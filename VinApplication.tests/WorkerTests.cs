using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using VinData;
using VinData.Models;
using VinDecoderWorker;

namespace VinApplication.Tests;

public class WorkerTests
{
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        context.Records.AddRange(
            new Record { Vin = "TESTVIN1", DealerId = "123", ModifiedDate = DateTime.UtcNow },
            new Record { Vin = "TESTVIN2", DealerId = "123", ModifiedDate = DateTime.UtcNow }
        );
        context.SaveChanges();

        return context;
    }

    private HttpClient CreateMockHttpClient()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new NhtsaResponse
                {
                    Results = new List<NhtsaResult>
                    {
                    new NhtsaResult { Variable = "Make", Value = "Chevrolet" },
                    new NhtsaResult { Variable = "Model", Value = "Impala" },
                    new NhtsaResult { Variable = "Model Year", Value = "2022" }
                    }
                })
            });

        return new HttpClient(handlerMock.Object);
    }


    [Fact]
    public async Task ProcessBatchAsync_AddsDecodedVehiclesToDb()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var services = new ServiceCollection()
            .AddSingleton(dbContext)
            .BuildServiceProvider();

        var logger = Mock.Of<ILogger<Worker>>();
        var httpClient = CreateMockHttpClient();
        var worker = new Worker(services, logger, httpClient);

        // Act
        await worker.ProcessBatchAsync(CancellationToken.None);

        // Assert
        var decoded = dbContext.DecodedVehicles.ToList();
        Assert.Equal(2, decoded.Count);
        Assert.All(decoded, v =>
        {
            Assert.Equal("Chevrolet", v.Make);
            Assert.Equal("Impala", v.Model);
            Assert.Equal("2022", v.Year);
        });
    }
}
