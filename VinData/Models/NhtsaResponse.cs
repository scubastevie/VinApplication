namespace VinData.Models;

public class NhtsaResponse
{
    public int Count { get; set; }
    public string Message { get; set; }
    public string SearchCriteria { get; set; }
    public List<NhtsaResult> Results { get; set; } = new();
}

public class NhtsaResult
{
    public string Value { get; set; }
    public string ValueId { get; set; }
    public string Variable { get; set; }
    public int VariableId { get; set; }
}
