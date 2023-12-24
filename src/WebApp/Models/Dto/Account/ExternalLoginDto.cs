using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Dto.Account;

public class ExternalLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}