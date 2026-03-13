using System.ComponentModel.DataAnnotations;

namespace AirWatch.Application.DTOs.Sensors;

public class RegisterSensorDto
{
    [Required(ErrorMessage = "ExternalId is required.")]
    [StringLength(100, ErrorMessage = "ExternalId must be at most 100 characters.")]
    public string ExternalId { get; init; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(150, ErrorMessage = "Name must be at most 150 characters.")]
    public string Name { get; init; } = string.Empty;

    [StringLength(200, ErrorMessage = "Location must be at most 200 characters.")]
    public string Location { get; init; } = string.Empty;
}
