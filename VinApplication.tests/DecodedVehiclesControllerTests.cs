using Microsoft.EntityFrameworkCore;
using VinData;
using VinData.Models;
using Microsoft.AspNetCore.Mvc;

namespace VinApplication.Tests
{
    public class DecodedVehiclesControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            context.DecodedVehicles.AddRange(
                new DecodedVehicle { Id = 1, Vin = "123", Make = "Ford", Model = "F-150", Year = "2020", RetrievedAt = DateTime.UtcNow },
                new DecodedVehicle { Id = 2, Vin = "456", Make = "Tesla", Model = "Model 3", Year = "2022", RetrievedAt = DateTime.UtcNow }
            );
            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task GetAllDecodedVehicles_ReturnsAll()
        {
            var context = GetInMemoryDbContext();
            var controller = new DecodedVehiclesController(context);

            var result = await controller.GetAllDecodedVehicles();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var vehicles = Assert.IsType<List<DecodedVehicle>>(okResult.Value);
            Assert.Equal(2, vehicles.Count);
        }

        [Fact]
        public async Task GetByVin_ExistingVin_ReturnsVehicle()
        {
            var context = GetInMemoryDbContext();
            var controller = new DecodedVehiclesController(context);

            var result = await controller.GetByVin("123");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var vehicle = Assert.IsType<DecodedVehicle>(okResult.Value);
            Assert.Equal("Ford", vehicle.Make);
        }

        [Fact]
        public async Task GetByVin_NonExistingVin_ReturnsNotFound()
        {
            var context = GetInMemoryDbContext();
            var controller = new DecodedVehiclesController(context);

            var result = await controller.GetByVin("000");

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
