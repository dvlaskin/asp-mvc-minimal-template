namespace WebApp.Models.Dto.AdminEntity;

public class EntityFieldDto
{
    public string Name { get; set; } = string.Empty;
    public string FieldFrontEndType { get; set; } = string.Empty;
    public Type FieldBackEndType { get; set; } = typeof(object);
    public object? Value { get; set; }

    public bool IsFloatingPointNumber =>
        FieldBackEndType == typeof(float)
        || FieldBackEndType == typeof(double)
        || FieldBackEndType == typeof(decimal);
}