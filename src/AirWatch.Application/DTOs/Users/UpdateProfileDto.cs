using System.ComponentModel.DataAnnotations;

namespace AirWatch.Application.DTOs.Users;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Name { get; init; } = string.Empty;

    public bool EmailNotificationsEnabled { get; init; } = true;
}
