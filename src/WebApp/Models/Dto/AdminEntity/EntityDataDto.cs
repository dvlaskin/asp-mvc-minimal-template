namespace WebApp.Models.Dto.AdminEntity;

public class EntityDataDto
{
    public string EntityName { get; set; } = string.Empty;
    public List<IReadOnlyList<(string Name, object Value, string TypeValue)>> EntityData { get; set; } = new();
}