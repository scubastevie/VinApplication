namespace VinData.Models;

public class DecodedVehicle
{
    public int Id { get; set; }
    public string? Vin { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Year { get; set; }
    public DateTime RetrievedAt { get; set; }
}
