using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Dto.AccountManager;

public class UserInfoManagerDto
{
    public string? Username { get; set; }

    public bool IsEmailConfirmed { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string? StatusMessage { get; set; }
}