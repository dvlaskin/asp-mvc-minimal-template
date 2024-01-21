namespace WebApp.Domain.Attributes
{
    /// <summary>
    /// Attribute for marking entity property as editable by admin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditableByAdminAttribute : Attribute
    {
    }
}