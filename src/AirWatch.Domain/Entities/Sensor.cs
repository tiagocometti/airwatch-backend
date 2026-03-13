namespace AirWatch.Domain.Entities;

public class Sensor
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }

    public ICollection<Measurement> Measurements { get; set; } = [];
}
