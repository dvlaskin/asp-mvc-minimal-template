using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApp.Db;
using WebApp.Models.Dto.AdminEntity;

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

        var entityDbSet = typeof(AppDbContext)
            .GetProperties()
            .First(p =>
                p.PropertyType.IsGenericType
                && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                && p.Name == entityName)
            .GetValue(appDbContext, null);

        if (entityDbSet is not null)
        {
            var entity = await ((IQueryable<dynamic>)entityDbSet).ToListAsync();

            var internalData = PrepareDataToDisplay(entity);
            internalData.EntityName = entityName;

            return View(internalData);
        }

        return View();
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

    private EntityDataDto PrepareDataToDisplay(IReadOnlyList<dynamic> data)
    {
        var internalData = new EntityDataDto();

        foreach (var item in data)
        {
            var dataType = item.GetType();
            var properties = dataType.GetProperties();
            var values = new List<(string Name, object Value, Type TypeValue)>();
            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(item, null);
                values.Add((property.Name, propertyValue?.ToString(), property.PropertyType));
            }
            internalData.EntityData.Add(values);
        }

        // var jsonStr = JsonConvert.SerializeObject(internalData, Formatting.Indented);
        // logger.LogInformation($"=> JSON:\r\n{jsonStr}");

        return internalData;
    }
}