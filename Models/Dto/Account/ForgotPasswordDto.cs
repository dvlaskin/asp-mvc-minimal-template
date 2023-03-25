using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Dto.Account;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}