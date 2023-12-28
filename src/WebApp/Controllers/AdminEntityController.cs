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
    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string entityName, string entityData)
    {
        return View();
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

    [HttpPut]
    public async Task<IActionResult> Edit(string entityName, EntityDataDto entityData)
    {
        return View();
    }


    [HttpGet("{entityName}/{id}")]
    public async Task<IActionResult> Delete(string entityName, int id)
    {
        return View();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string entityName, string entityData)
    {
        return View();
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
            var values = new List<(string Name, object Value, string TypeValue)>();
            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(item, null);
                values.Add((property.Name, propertyValue?.ToString(), property.PropertyType.ConvertToHtmlInputType()));
            }
            internalData.EntityData.Add(values);
        }

        // var jsonStr = JsonConvert.SerializeObject(internalData, Formatting.Indented);
        // logger.LogInformation($"=> JSON:\r\n{jsonStr}");

        return internalData;
    }
}