namespace AirWatch.Domain.Entities;

public class Measurement
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public string SensorType { get; set; } = string.Empty; // "mq3", "mq5", "mq135"
    public bool Calibrated { get; set; }
    public int AdcRaw { get; set; }
    public double VoltageV { get; set; }
    public double RsOhm { get; set; }
    public double RsR0Ratio { get; set; }
    public double Ppm { get; set; }
    public DateTime Timestamp { get; set; }
}
