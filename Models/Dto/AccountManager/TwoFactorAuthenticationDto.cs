namespace WebApp.Models.Dto.AccountManager;

public class TwoFactorAuthenticationDto
{
    public bool HasAuthenticator { get; set; }

    public int RecoveryCodesLeft { get; set; }

    public bool Is2faEnabled { get; set; }
}