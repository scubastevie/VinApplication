using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Globalization;

[ApiController]
[Route("api/[controller]")]
public class CsvUploadController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public CsvUploadController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _blobServiceClient = new BlobServiceClient(configuration["AzureStorage:ConnectionString"]);
        _containerName = configuration["AzureStorage:ContainerName"];
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("CSV file is required.");

        // ✅ Step 1: Upload raw file to Blob Storage
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobName = $"{Guid.NewGuid()}_{file.FileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        using var uploadStream = file.OpenReadStream();
        await blobClient.UploadAsync(uploadStream, overwrite: true);

        // ✅ Step 2: Rewind stream and parse contents
        uploadStream.Position = 0;
        using var reader = new StreamReader(uploadStream);

        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
            return BadRequest("CSV file is empty.");

        var headers = headerLine.Split(',').Select(h => h.Trim().ToLower()).ToArray();

        var dealerIdIndex = Array.IndexOf(headers, "dealerid");
        var vinIndex = Array.IndexOf(headers, "vin");
        var modifiedDateIndex = Array.IndexOf(headers, "modifieddate");

        if (dealerIdIndex < 0 || vinIndex < 0 || modifiedDateIndex < 0)
        {
            return BadRequest("CSV must contain headers: dealerId, vin, modifiedDate.");
        }

        var records = new List<Record>();
        string? line;
        int lineNumber = 1;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            var values = line.Split(',');

            if (values.Length <= Math.Max(dealerIdIndex, Math.Max(vinIndex, modifiedDateIndex)))
                return BadRequest($"Missing columns on line {lineNumber}.");

            var dealerId = values[dealerIdIndex].Trim();
            var vin = values[vinIndex].Trim();
            var modifiedDateStr = values[modifiedDateIndex].Trim();

            if (string.IsNullOrEmpty(dealerId) || string.IsNullOrEmpty(vin) || string.IsNullOrEmpty(modifiedDateStr))
                return BadRequest($"One or more required fields are empty on line {lineNumber}.");

            if (!DateTime.TryParse(modifiedDateStr, null, DateTimeStyles.AssumeUniversal, out var parsedDate))
                return BadRequest($"Invalid date format on line {lineNumber}: '{modifiedDateStr}'.");

            records.Add(new Record
            {
                DealerId = dealerId,
                Vin = vin,
                ModifiedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc)
            });
        }

        _context.Records.AddRange(records);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = $"Uploaded {records.Count} records successfully.",
            BlobUrl = blobClient.Uri.ToString()
        });
    }
}
