using Microsoft.AspNetCore.Mvc;
using System;

[ApiController]
[Route("api/[controller]")]
public class CsvUploadController : ControllerBase
{
    private readonly AppDbContext _context;

    public CsvUploadController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("CSV file is required.");
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return BadRequest("CSV file is empty.");
        }

        var headers = headerLine.Split(',').Select(h => h.Trim().ToLower()).ToArray();

        var dealerIdIndex = Array.IndexOf(headers, "dealerid");
        var vinIndex = Array.IndexOf(headers, "vin");
        var modifiedDateIndex = Array.IndexOf(headers, "modifieddate");

        if (dealerIdIndex < 0 || vinIndex < 0 || modifiedDateIndex < 0)
        {
            return BadRequest("CSV must contain headers: dealerId, vin, modifiedDate.");
        }

        var records = new List<Record>();
        string line;
        int lineNumber = 1;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            var values = line.Split(',');

            if (values.Length <= Math.Max(dealerIdIndex, Math.Max(vinIndex, modifiedDateIndex)))
            {
                return BadRequest($"Missing columns on line {lineNumber}.");
            }

            var dealerId = values[dealerIdIndex].Trim();
            var vin = values[vinIndex].Trim();
            var modifiedDate = values[modifiedDateIndex].Trim();

            if (string.IsNullOrEmpty(dealerId) || string.IsNullOrEmpty(vin) || string.IsNullOrEmpty(modifiedDate))
            {
                return BadRequest($"One or more required fields are empty on line {lineNumber}.");
            }

            if (!DateTime.TryParse(modifiedDate, out var parsedDate))
            {
                return BadRequest($"Invalid date format on line {lineNumber}.");
            }

            records.Add(new Record
            {
                DealerId = dealerId,
                Vin = vin,
                ModifiedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc)
            });

        }

        _context.Records.AddRange(records);
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Uploaded {records.Count} records successfully." });
    }

}
