using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http.Json;
using VinData;
using VinData.Models;

namespace VinDecoderWorker;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<Worker> _logger;
    private readonly HttpClient _httpClient = new();

    public Worker(IServiceProvider services, ILogger<Worker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var vins = await db.Records
                .Where(r => !db.DecodedVehicles.Any(d => d.Vin == r.Vin))
                .Select(r => r.Vin)
                .Distinct()
                .Take(10)
                .ToListAsync(stoppingToken);

            foreach (var vin in vins)
            {
                var response = await _httpClient.GetFromJsonAsync<NhtsaResponse>(
                    $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevin/{vin}?format=json", cancellationToken: stoppingToken);

                if (response?.Results != null)
                {
                    db.DecodedVehicles.Add(new DecodedVehicle
                    {
                        Vin = vin,
                        Make = response.Results.FirstOrDefault(r => r.Variable == "Make")?.Value,
                        Model = response.Results.FirstOrDefault(r => r.Variable == "Model")?.Value,
                        Year = response.Results.FirstOrDefault(r => r.Variable == "Model Year")?.Value,
                        RetrievedAt = DateTime.UtcNow
                    });
                }

                await Task.Delay(1000, stoppingToken); // Avoid rate limits
            }

            await db.SaveChangesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
