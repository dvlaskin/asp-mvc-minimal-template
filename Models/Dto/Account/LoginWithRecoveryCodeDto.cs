using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Dto.Account;

public class LoginWithRecoveryCodeDto
{
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Recovery Code")]
    public string RecoveryCode { get; set; }
}