namespace Swimago.Domain.Entities;

public class City
{
    public Guid Id { get; set; }
    public Dictionary<string, string> Name { get; set; } = new(); // Multi-language
    public string Country { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
