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

    // RL (resistor de carga) individual por sensor
    public double RlMq3   { get; set; } = 10000.0;
    public double RlMq5   { get; set; } = 10000.0;
    public double RlMq135 { get; set; } = 10000.0;

    // R0 (resistência em ar limpo) — temporário até implementação de calibração
    public double R0Mq3   { get; set; } = 25000.0;
    public double R0Mq5   { get; set; } = 105000.0;
    public double R0Mq135 { get; set; } = 76630.0;

    public ICollection<Measurement> Measurements { get; set; } = [];
}
