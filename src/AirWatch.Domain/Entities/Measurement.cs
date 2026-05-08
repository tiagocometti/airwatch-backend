namespace AirWatch.Domain.Entities;

public class Measurement
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public int Mq3Adc    { get; set; }
    public int Mq5Adc    { get; set; }
    public int Mq135Adc  { get; set; }

    public double PpmAlcohol { get; set; }
    public double PpmLpg     { get; set; }
    public double PpmCo2     { get; set; }
    public double PpmNh3     { get; set; }
}
