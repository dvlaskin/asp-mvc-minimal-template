namespace WebApp.Models.Dto.AdminEntity;

public class EntityDataDto
{
    public string EntityName { get; set; } = string.Empty;
    public List<IReadOnlyList<EntityFieldDto>> EntityData { get; set; } = new();
}