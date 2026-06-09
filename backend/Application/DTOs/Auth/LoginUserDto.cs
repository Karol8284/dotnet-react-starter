using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class LoginUserDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}
