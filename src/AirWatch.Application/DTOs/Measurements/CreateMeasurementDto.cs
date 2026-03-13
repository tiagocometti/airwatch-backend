using System.ComponentModel.DataAnnotations;

namespace AirWatch.Application.DTOs.Measurements;

public class CreateMeasurementDto
{
    [Required(ErrorMessage = "O identificador do sensor (sensorId) é obrigatório.")]
    [StringLength(100, ErrorMessage = "O identificador do sensor deve ter no máximo 100 caracteres.")]
    public string SensorId { get; init; } = string.Empty;

    [Range(-100, 10000, ErrorMessage = "O valor de gás deve estar entre -100 e 10000.")]
    public double GasValue { get; init; }

    [Range(-50, 100, ErrorMessage = "A temperatura deve estar entre -50°C e 100°C.")]
    public double Temperature { get; init; }

    [Range(0, 100, ErrorMessage = "A umidade deve estar entre 0% e 100%.")]
    public double Humidity { get; init; }

    [Required(ErrorMessage = "O timestamp da medição é obrigatório.")]
    public DateTime Timestamp { get; init; }
}
