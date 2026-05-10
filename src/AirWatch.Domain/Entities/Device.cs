namespace AirWatch.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
    public DateTime RegisteredAt { get; set; }

    // RL (resistor de carga) individual por sensor — propriedade do hardware
    public double RlMq3   { get; set; } = 10000.0;
    public double RlMq5   { get; set; } = 10000.0;
    public double RlMq135 { get; set; } = 10000.0;

    public ICollection<Measurement> Measurements { get; set; } = [];
    public ICollection<Calibration> Calibrations { get; set; } = [];
}
