using WebApp.Models.Dto.AdminEntity;

namespace WebApp.Domain.Interfaces;

/// <summary>
/// Provides functionality for editing entities by an admin
/// </summary>
public interface IAdminEntityEditor
{
    /// <summary>
    /// Get list of tables in database, which can be edited by admin
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<string> GetTablesList();

    /// <summary>
    /// Get DbSet by Entity(Table) name
    /// </summary>
    /// <param name="entityName"></param>
    /// <returns></returns>
    object? GetDbSetByEntityName(string entityName);

    /// <summary>
    /// Get all records from DbSet by Entity(Table) name
    /// </summary>
    /// <param name="entityName"></param>
    /// <returns></returns>
    Task<EntityDataDto> GetAllRecordFromDbSetAsync(string entityName);

    /// <summary>
    /// Get Entity(Table) model with default values of the fields
    /// </summary>
    /// <param name="entityName"></param>
    /// <returns></returns>
    EntityDataDto GetDefaultNewRecordModel(string entityName);

    /// <summary>
    /// Get record from DbSet by Entity(Table) name and id
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<EntityDataDto> GetRecordValuesAsync(string entityName, string id);


    /// <summary>
    /// Create new record in DbSet by Entity(Table) name and record fields
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="recordFields"></param>
    /// <returns></returns>
    Task CreateEntityRecordAsync(string entityName, Dictionary<string, string> recordFields);

    /// <summary>
    /// Update record in DbSet by Entity(Table) name and record fields
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="recordFields"></param>
    /// <returns></returns>
    Task UpdateRecordAsync(string entityName, Dictionary<string, string> recordFields);

    /// <summary>
    /// Delete item from DbSet by Entity(Table) name and id
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteItemAsync(string entityName, string id);
}