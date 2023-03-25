using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Models.Dto.AccountManager;

public class ExternalLoginsDto
{
    public IList<UserLoginInfo> CurrentLogins { get; set; }

    public IList<AuthenticationScheme> OtherLogins { get; set; }

    public bool ShowRemoveButton { get; set; }

    public string StatusMessage { get; set; }
}