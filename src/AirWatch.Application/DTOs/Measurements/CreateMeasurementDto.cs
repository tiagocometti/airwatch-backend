namespace AirWatch.Application.DTOs.Measurements;

public class CreateMeasurementDto
{
    public Guid DeviceId { get; set; }
    public string SensorType { get; set; } = string.Empty;
    public bool Calibrated { get; set; }
    public int AdcRaw { get; set; }
    public double VoltageV { get; set; }
    public double RsOhm { get; set; }
    public double RsR0Ratio { get; set; }
    public double Ppm { get; set; }
    public DateTime Timestamp { get; set; }
}
