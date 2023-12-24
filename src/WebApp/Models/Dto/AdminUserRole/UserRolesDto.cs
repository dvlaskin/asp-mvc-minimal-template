namespace WebApp.Models.Dto.AdminUserRole;

public class UserRolesDto
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string? AddRole { get; set; }
    public string? RemoveRole { get; set; }
    public IEnumerable<string> UserRoles { get; set; }
    public IEnumerable<string> AllRoles { get; set; }

}