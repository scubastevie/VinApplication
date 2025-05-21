using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinData;

[ApiController]
[Route("api/[controller]")]
public class VinController : ControllerBase
{
    private readonly AppDbContext _context;

    public VinController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("vins")]
    public async Task<ActionResult<IEnumerable<string>>> GetAllVins()
    {
        var vins = await _context.Records
            .Select(r => r.Vin)
            .Distinct()
            .ToListAsync();

        return Ok(vins);
    }

    [HttpGet]
    [Route("all-records")]
    public async Task<ActionResult<IEnumerable<Record>>> GetAllRecords()
    {
        var records = await _context.Records.ToListAsync();
        return Ok(records);
    }
}
