namespace WebApp.Domain.Models;

public class ActionLinkDescription
{
    public string Action { get; set; }
    public string Controller { get; set; }
    public string UserId { get; set; }
    public string Code { get; set; }
    public string Scheme { get; set; }
}