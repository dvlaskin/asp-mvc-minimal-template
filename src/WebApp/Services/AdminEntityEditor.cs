using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebApp.Db;
using WebApp.Domain.Attributes;
using WebApp.Domain.Extensions;
using WebApp.Domain.Interfaces;
using WebApp.Models.Dto.AdminEntity;

namespace WebApp.Services;

/// <inheritdoc/>
public class AdminEntityEditor : IAdminEntityEditor
{
    private readonly AppDbContext dbContext;

    public AdminEntityEditor(AppDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetTablesList()
    {
        var result = new List<string>();

        var dbSetProperties = dbContext
            .GetType()
            .GetProperties()
            .Where(p =>
                p.PropertyType.IsGenericType
                && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                && p.GetCustomAttributes(typeof(EditableByAdminAttribute), true).Any())
            .ToArray();

        if (dbSetProperties.Any())
        {
            var propertiesNames = dbSetProperties.Select(s => s.Name);
            result.AddRange(propertiesNames);
        }

        return result;
    }

    /// <inheritdoc/>
    public object? GetDbSetByEntityName(string entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new ArgumentNullException(nameof(entityName));
        }

        return dbContext
            .GetType()
            .GetProperties()
            .First(p =>
                p.PropertyType.IsGenericType
                && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                && p.Name == entityName)
            .GetValue(dbContext, null);
    }

    /// <inheritdoc/>
    public async Task<EntityDataDto> GetAllRecordFromDbSetAsync(string entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new ArgumentNullException(nameof(entityName));
        }

        object? entityDbSet = GetDbSetByEntityName(entityName);

        if (entityDbSet is null)
        {
            return new EntityDataDto();
        }

        IReadOnlyList<object> entity = await GetAllRecordFromDbSet(entityDbSet);

        var internalData = PrepareDataToDisplay(entity);
        internalData.EntityName = entityName;

        return internalData;
    }

    /// <inheritdoc/>
    public EntityDataDto GetDefaultNewRecordModel(string entityName)
    {
        var model = new EntityDataDto { EntityName = entityName };

        object? entityDbSet = GetDbSetByEntityName(entityName);

        // get properties of the specified entityName
        PropertyInfo[]? entityProperties = entityDbSet?
            .GetType()
            .GetGenericArguments()[0]
            .GetProperties();

        if (entityProperties is null)
        {
            return model;
        }

        var defaultValues = new List<EntityFieldDto>();

        // set default values for each property and add them to the model
        foreach (var property in entityProperties)
        {
            var propertyBackType = property.PropertyType;
            var propertyFrontType = propertyBackType.ConvertToHtmlInputType();
            defaultValues.Add(new EntityFieldDto
            {
                Name = property.Name,
                Value = null,
                FieldFrontEndType = propertyFrontType,
                FieldBackEndType = propertyBackType
            });
        }
        model.EntityData.Add(defaultValues);

        return model;
    }

    /// <inheritdoc/>
    public async Task<EntityDataDto> GetRecordValuesAsync(string entityName, string id)
    {
        object? entityDbSet = GetDbSetByEntityName(entityName);

        if (entityDbSet is not null)
        {
            IReadOnlyList<object> entity = await GetFilteredRecordsFromDbSet(entityDbSet, "Id", id);

            var recordData = PrepareDataToDisplay(entity);
            recordData.EntityName = entityName;

            return recordData;
        }

        return new EntityDataDto { EntityName = entityName };
    }

    /// <inheritdoc/>
    public async Task CreateEntityRecordAsync(string entityName, Dictionary<string, string> recordFields)
    {
        EntityRecordValidation(entityName, recordFields);

        // get from DbContext the DbSet for the specified entityName
        object? entityDbSet = GetDbSetByEntityName(entityName);

        // create an instance generic type of the dbset
        var entityType = (entityDbSet?.GetType().GetGenericArguments()[0])
            ?? throw new InvalidOperationException($"Entity type for {entityName} is null");

        var entityInstance = Activator.CreateInstance(entityType)
            ?? throw new InvalidOperationException($"Entity instance for {entityName} is null");

        SetPropertyValues(recordFields, entityType, entityInstance);

        // call the Add method of the DbSet
        entityDbSet?.GetType().GetMethod("Add")?.Invoke(entityDbSet, new[] { entityInstance });

        await dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateRecordAsync(string entityName, Dictionary<string, string> recordFields)
    {
        EntityRecordValidation(entityName, recordFields);

        // get from DbContext the DbSet for the specified entityName
        object? entityDbSet = GetDbSetByEntityName(entityName);

        // create an instance generic type of the dbset
        var entityType = entityDbSet?.GetType().GetGenericArguments()[0]
            ?? throw new InvalidOperationException($"Generic type for {entityName} is null");

        var entityInstance = Activator.CreateInstance(entityType)
            ?? throw new InvalidOperationException($"Entity instance for {entityName} is null");

        SetPropertyValues(recordFields, entityType, entityInstance);

        // call the Update method of the DbSet
        entityDbSet?.GetType().GetMethod("Update")?.Invoke(entityDbSet, new[] { entityInstance });

        await dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteItemAsync(string entityName, string id)
    {
        if (string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        object? entityDbSet = GetDbSetByEntityName(entityName);

        // call the Find method of the DbSet to get the entity with the specified id
        var entity = entityDbSet?.GetType().GetMethod("Find")?.Invoke(entityDbSet, new object[] { new object[] { id } });

        // if the entity is found, call the Remove method of the DbSet
        if (entity is not null)
        {
            entityDbSet?.GetType().GetMethod("Remove")?.Invoke(entityDbSet, new object[] { entity });
        }

        await dbContext.SaveChangesAsync();
    }


    private static EntityDataDto PrepareDataToDisplay(IReadOnlyList<object> data)
    {
        var internalData = new EntityDataDto();

        foreach (var item in data)
        {
            var dataType = item.GetType();
            var properties = dataType.GetProperties();
            var values = new List<EntityFieldDto>();
            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(item, null);
                values.Add(new EntityFieldDto
                {
                    Name = property.Name,
                    Value = propertyValue,
                    FieldFrontEndType = property.PropertyType.ConvertToHtmlInputType(),
                    FieldBackEndType = property.PropertyType
                });
            }
            internalData.EntityData.Add(values);
        }

        return internalData;
    }

    private static async Task<IReadOnlyList<object>> GetAllRecordFromDbSet(object? entityDbSet)
    {
        var result = new List<object>();

        if (entityDbSet is not null && entityDbSet is IQueryable<object> queryable)
        {
            result = await queryable.ToListAsync();
        }

        return result;
    }

    private static async Task<IReadOnlyList<object>> GetFilteredRecordsFromDbSet(object? entityDbSet, string propertyName, string propertyValue)
    {
        var result = new List<object>();

        if (entityDbSet is null)
        {
            return result;
        }

        if (entityDbSet is IQueryable<object> queryableDbSet)
        {
            if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(propertyValue))
            {
                var allRecords = await queryableDbSet.ToArrayAsync();
                var filteredRecords = allRecords.Where(e => FilterByProperty((object)e, propertyName, propertyValue)).ToList();
                result.AddRange(filteredRecords);
            }
        }

        return result;
    }

    private static bool FilterByProperty(object entity, string propertyName, string propertyValue)
    {
        var result = false;

        if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(propertyValue))
        {
            var property = entity.GetType().GetProperty(propertyName);
            if (property is not null)
            {
                var propertyValueStr = property.GetValue(entity, null)?.ToString();
                if (propertyValueStr is not null)
                {
                    result = propertyValueStr == propertyValue;
                }
            }
        }

        return result;
    }

    private static void SetPropertyValues(Dictionary<string, string> recordFields, Type entityType, object entityInstance)
    {
        // set properties of the generic instance with the values from the form
        foreach (var property in entityType.GetProperties())
        {
            if (recordFields.ContainsKey(property.Name))
            {
                var propertyValue = recordFields[property.Name];

                if (property.PropertyType == typeof(decimal))
                {
                    propertyValue = propertyValue.Replace(".", ",");
                }

                object convertedValue;

                // Special case for boolean properties
                if (property.PropertyType == typeof(bool))
                {
                    convertedValue = propertyValue == "true" || propertyValue == "on";
                }
                else
                {
                    // Convert the string value to the appropriate data type
                    convertedValue = Convert.ChangeType(propertyValue, property.PropertyType);
                }

                // generate Id for the entity if it is not set
                if (property.Name == "Id" && convertedValue is null)
                {
                    convertedValue = Guid.NewGuid().ToString();
                }

                property.SetValue(entityInstance, convertedValue);
            }
        }
    }

    private static void EntityRecordValidation(string entityName, Dictionary<string, string> recordFields)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new ArgumentNullException(nameof(entityName));
        }

        if (recordFields is null || !recordFields.Any())
        {
            throw new ArgumentException("The recordFields is empty", nameof(recordFields));
        }
    }
}
