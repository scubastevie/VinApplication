using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinData;
using VinData.Models;

[ApiController]
[Route("api/[controller]")]
public class DecodedVehiclesController : ControllerBase
{
    private readonly AppDbContext _context;

    public DecodedVehiclesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("all")]
    public async Task<ActionResult<IEnumerable<DecodedVehicle>>> GetAllDecodedVehicles()
    {
        var vehicles = await _context.DecodedVehicles.ToListAsync();
        return Ok(vehicles);
    }

    [HttpGet]
    [Route("by-vin/{vin}")]
    public async Task<ActionResult<DecodedVehicle>> GetByVin(string vin)
    {
        var vehicle = await _context.DecodedVehicles
            .FirstOrDefaultAsync(v => v.Vin == vin);

        if (vehicle == null)
            return NotFound($"No decoded vehicle found with VIN: {vin}");

        return Ok(vehicle);
    }
}
