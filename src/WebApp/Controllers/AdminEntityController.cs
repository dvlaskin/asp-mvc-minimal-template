using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApp.Db;
using WebApp.Models.Dto.AdminEntity;
using WebApp.Domain.Extensions;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]/[action]")]
public class AdminEntityController : Controller
{
    private readonly AppDbContext appDbContext;
    private readonly ILogger<AdminEntityController> logger;

    public AdminEntityController(
        AppDbContext appDbContext,
        ILogger<AdminEntityController> logger)
    {
        this.appDbContext = appDbContext;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tablesList = GetTablesList();
        ViewBag.TablesList = tablesList;

        // Handle error case when the specified entity is not found
        return View();
    }

    [HttpGet("{entityName}")]
    public async Task<IActionResult> Index(string entityName)
    {
        var tablesList = GetTablesList();
        ViewBag.TablesList = tablesList;

        object? entityDbSet = GetDbSetByEntityName(entityName);

        if (entityDbSet is not null)
        {
            IReadOnlyList<object> entity = await GetAllRecordFromDbSet(entityDbSet);

            var internalData = PrepareDataToDisplay(entity);
            internalData.EntityName = entityName;

            return View(internalData);
        }

        return View();
    }


    [HttpGet]
    public async Task<IActionResult> Create(string entityName)
    {
        var tablesList = GetTablesList();
        ViewBag.TablesList = tablesList;

        var model = new EntityDataDto { EntityName = entityName };

        // get from DbContext the DbSet for the specified entityName
        object? entityDbSet = GetDbSetByEntityName(entityName);

        // get properties of the specified entityName
        var entityProperties = entityDbSet?
            .GetType()
            .GetGenericArguments()[0]
            .GetProperties();

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

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string entityName, [FromForm] Dictionary<string, string> formData)
    {
        logger.LogInformation($"=> Create: EntityName = {entityName}");
        logger.LogInformation($"=> Create: {JsonConvert.SerializeObject(formData, Formatting.Indented)}");

        // get from DbContext the DbSet for the specified entityName
        object? entityDbSet = GetDbSetByEntityName(entityName);

        // create an instance generic type of the dbset
        var genericType = entityDbSet?.GetType().GetGenericArguments()[0];
        var genericInstance = Activator.CreateInstance(genericType);

        // set properties of the generic instance with the values from the form
        foreach (var property in genericType?.GetProperties())
        {
            if (formData.ContainsKey(property.Name))
            {
                var propertyValue = formData[property.Name];

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

                property.SetValue(genericInstance, convertedValue);
            }
        }

        logger.LogInformation($"=> Create: genericInstance\r\n{JsonConvert.SerializeObject(genericInstance, Formatting.Indented)}");

        // call the Add method of the DbSet
        entityDbSet?.GetType().GetMethod("Add")?.Invoke(entityDbSet, new[] { genericInstance });

        await appDbContext.SaveChangesAsync();

        return RedirectToAction("Index", new { entityName = entityName });
    }


    [HttpGet("{entityName}/{id}")]
    public async Task<IActionResult> Edit(string entityName, string id)
    {
        var tablesList = GetTablesList();
        ViewBag.TablesList = tablesList;

        logger.LogInformation($"=> Edit: {entityName} - {id}");

        object? entityDbSet = GetDbSetByEntityName(entityName);

        if (entityDbSet is not null)
        {
            IReadOnlyList<object> entity = await GetFilteredRecordsFromDbSet(entityDbSet, "Id", id);

            var internalData = PrepareDataToDisplay(entity);
            internalData.EntityName = entityName;

            return View(internalData);
        }

        return View(new EntityDataDto() { EntityName = entityName });
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] string entityName, [FromForm] Dictionary<string, string> formData)
    {
        logger.LogInformation($"=> EditPost: EntityName = {entityName}");
        logger.LogInformation($"=> EditPost: {JsonConvert.SerializeObject(formData, Formatting.Indented)}");

        // get from DbContext the DbSet for the specified entityName
        object? entityDbSet = GetDbSetByEntityName(entityName);

        // create an instance generic type of the dbset
        var genericType = entityDbSet?.GetType().GetGenericArguments()[0];
        var genericInstance = Activator.CreateInstance(genericType);

        // set properties of the generic instance with the values from the form
        foreach (var property in genericType?.GetProperties())
        {
            if (formData.ContainsKey(property.Name))
            {
                var propertyValue = formData[property.Name];

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

                property.SetValue(genericInstance, convertedValue);
            }
        }

        logger.LogInformation($"=> Update: genericInstance\r\n{JsonConvert.SerializeObject(genericInstance, Formatting.Indented)}");

        // call the Update method of the DbSet
        entityDbSet?.GetType().GetMethod("Update")?.Invoke(entityDbSet, new[] { genericInstance });

        await appDbContext.SaveChangesAsync();

        return RedirectToAction("Index", new { entityName = entityName });
    }


    [HttpGet("{entityName}/{id}")]
    public async Task<IActionResult> Delete(string entityName, string id)
    {
        logger.LogInformation($"=> Delete: {entityName} - {id}");

        // get from DbContext the DbSet for the specified entityName
        object? entityDbSet = GetDbSetByEntityName(entityName);

        // call the Find method of the DbSet to get the entity with the specified id
        var entity = entityDbSet?.GetType().GetMethod("Find")?.Invoke(entityDbSet, new object[] { new object[] { id } });

        logger.LogInformation($"=> Delete: found entity\r\n{JsonConvert.SerializeObject(entity, Formatting.Indented)}");

        // if the entity is found, call the Remove method of the DbSet
        if (entity is not null)
        {
            entityDbSet?.GetType().GetMethod("Remove")?.Invoke(entityDbSet, new object[] { entity });
        }

        await appDbContext.SaveChangesAsync();

        return RedirectToAction("Index", new { entityName = entityName });
    }


    private object? GetDbSetByEntityName(string entityName)
    {
        return typeof(AppDbContext)
            .GetProperties()
            .First(p =>
                p.PropertyType.IsGenericType
                && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                && p.Name == entityName)
            .GetValue(appDbContext, null);
    }

    private static async Task<IReadOnlyList<object>> GetAllRecordFromDbSet(object? entityDbSet)
    {
        var result = new List<object>();

        if (entityDbSet is null)
        {
            return result;
        }

        if (entityDbSet is IQueryable<object>)
        {
            result = await ((IQueryable<object>)entityDbSet).ToListAsync();
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

        if (entityDbSet is IQueryable<object>)
        {
            var queryableDbSet = (IQueryable<object>)entityDbSet;

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

    private IReadOnlyList<string> GetTablesList()
    {
        var result = new List<string>();

        var dbSetProperties = typeof(AppDbContext)
            .GetProperties()
            .Where(p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        if (dbSetProperties != null)
        {
            var propertiesNames = dbSetProperties.Select(s => s.Name).ToArray();
            result.AddRange(propertiesNames);
        }

        return result;
    }

    private EntityDataDto PrepareDataToDisplay(IReadOnlyList<object> data)
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

        // var jsonStr = JsonConvert.SerializeObject(internalData, Formatting.Indented);
        // logger.LogInformation($"=> JSON:\r\n{jsonStr}");

        return internalData;
    }
}