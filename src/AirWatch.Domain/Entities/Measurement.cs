namespace AirWatch.Domain.Entities;

public class Measurement
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public int Mq3Adc   { get; set; }
    public int Mq5Adc   { get; set; }
    public int Mq135Adc { get; set; }

    public double RatioMq3   { get; set; }
    public double RatioMq5   { get; set; }
    public double RatioMq135 { get; set; }

    public double DegMq3   { get; set; }
    public double DegMq5   { get; set; }
    public double DegMq135 { get; set; }

    public double Iqai         { get; set; }
    public string IqaiCategory { get; set; } = "Boa";
}
