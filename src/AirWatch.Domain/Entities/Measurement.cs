namespace AirWatch.Domain.Entities;

public class Measurement
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set; }
    public double GasValue { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public DateTime Timestamp { get; set; }

    public Sensor Sensor { get; set; } = null!;
}
