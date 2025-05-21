using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using VinData;
using VinData.Models;

namespace VinDecoderWorker;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<Worker> _logger;
    private readonly HttpClient _httpClient;

    public Worker(IServiceProvider services, ILogger<Worker> logger, HttpClient? httpClient = null)
    {
        _services = services;
        _logger = logger;
        _httpClient = httpClient ?? new HttpClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    public async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var allVins = await db.Records
            .Select(r => r.Vin)
            .Distinct()
            .ToListAsync(cancellationToken);

        var alreadyDecoded = await db.DecodedVehicles
            .Select(d => d.Vin)
            .ToListAsync(cancellationToken);

        var vinsToProcess = allVins
            .Where(v => !alreadyDecoded.Contains(v))
            .Take(10)
            .ToList();

        foreach (var vin in vinsToProcess)
        {
            var response = await _httpClient.GetFromJsonAsync<NhtsaResponse>(
                $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevin/{vin}?format=json", cancellationToken);

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

            await Task.Delay(1000, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
