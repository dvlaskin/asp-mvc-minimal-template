namespace WebApp.Models.Dto.AccountManager;

public class RemoveLoginDto
{
    public string LoginProvider { get; set; }
    public string ProviderKey { get; set; }
}