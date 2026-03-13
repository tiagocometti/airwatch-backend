using System.ComponentModel.DataAnnotations;

namespace AirWatch.Application.DTOs.Measurements;

public class CreateMeasurementDto
{
    [Required(ErrorMessage = "SensorId is required.")]
    [StringLength(100, ErrorMessage = "SensorId must be at most 100 characters.")]
    public string SensorId { get; init; } = string.Empty;

    [Range(-100, 10000, ErrorMessage = "GasValue must be between -100 and 10000.")]
    public double GasValue { get; init; }

    [Range(-50, 100, ErrorMessage = "Temperature must be between -50°C and 100°C.")]
    public double Temperature { get; init; }

    [Range(0, 100, ErrorMessage = "Humidity must be between 0% and 100%.")]
    public double Humidity { get; init; }

    [Required(ErrorMessage = "Timestamp is required.")]
    public DateTime Timestamp { get; init; }
}
