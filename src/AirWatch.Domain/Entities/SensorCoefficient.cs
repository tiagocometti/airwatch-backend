namespace AirWatch.Domain.Entities;

public class SensorCoefficient
{
    public Guid   Id         { get; set; }
    public string SensorType { get; set; } = string.Empty; // "MQ3", "MQ5", "MQ135"
    public string GasTarget  { get; set; } = string.Empty; // "Alcohol", "LPG", "CO2", "NH3"
    public double CoefA      { get; set; }
    public double CoefB      { get; set; }
    public double RatioMin   { get; set; }
    public double RatioMax   { get; set; }
}
