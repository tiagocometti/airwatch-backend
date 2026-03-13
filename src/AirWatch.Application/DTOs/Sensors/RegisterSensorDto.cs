using System.ComponentModel.DataAnnotations;

namespace AirWatch.Application.DTOs.Sensors;

public class RegisterSensorDto
{
    [Required(ErrorMessage = "O identificador externo do sensor (externalId) é obrigatório.")]
    [StringLength(100, ErrorMessage = "O identificador externo deve ter no máximo 100 caracteres.")]
    public string ExternalId { get; init; } = string.Empty;

    [Required(ErrorMessage = "O nome do sensor é obrigatório.")]
    [StringLength(150, ErrorMessage = "O nome do sensor deve ter no máximo 150 caracteres.")]
    public string Name { get; init; } = string.Empty;

    [StringLength(200, ErrorMessage = "A localização deve ter no máximo 200 caracteres.")]
    public string Location { get; init; } = string.Empty;
}
