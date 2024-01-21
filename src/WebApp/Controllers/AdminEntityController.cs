using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApp.Domain.Interfaces;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]/[action]")]
public class AdminEntityController : Controller
{
    private readonly IAdminEntityEditor entityEditor;
    private readonly ILogger<AdminEntityController> logger;

    public AdminEntityController(
        IAdminEntityEditor entityEditor,
        ILogger<AdminEntityController> logger)
    {
        this.entityEditor = entityEditor;
        this.logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.TablesList = entityEditor.GetTablesList();
        return View();
    }

    [HttpGet("{entityName}")]
    public async Task<IActionResult> Index(string entityName)
    {
        ViewBag.TablesList = entityEditor.GetTablesList();

        var entityRecords = await entityEditor.GetAllRecordFromDbSetAsync(entityName);

        return View(entityRecords);
    }


    [HttpGet]
    public IActionResult Create(string entityName)
    {
        ViewBag.TablesList = entityEditor.GetTablesList();

        var model = entityEditor.GetDefaultNewRecordModel(entityName);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string entityName, [FromForm] Dictionary<string, string> formData)
    {
        logger.LogInformation($"=> Create: EntityName = {entityName}");
        logger.LogInformation($"=> Create: {JsonConvert.SerializeObject(formData, Formatting.Indented)}");

        await entityEditor.CreateEntityRecordAsync(entityName, formData);

        return RedirectToAction("Index", new { entityName = entityName });
    }


    [HttpGet("{entityName}/{id}")]
    public async Task<IActionResult> Edit(string entityName, string id)
    {
        ViewBag.TablesList = entityEditor.GetTablesList();

        logger.LogInformation($"=> Edit: {entityName} - {id}");

        var entityRecord = await entityEditor.GetRecordValuesAsync(entityName, id);
        if (entityRecord is null)
        {
            return NotFound();
        }

        return View(entityRecord);
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] string entityName, [FromForm] Dictionary<string, string> formData)
    {
        logger.LogInformation($"=> EditPost: EntityName = {entityName}");
        logger.LogInformation($"=> EditPost: {JsonConvert.SerializeObject(formData, Formatting.Indented)}");

        await entityEditor.UpdateRecordAsync(entityName, formData);

        return RedirectToAction("Index", new { entityName = entityName });
    }


    [HttpGet("{entityName}/{id}")]
    public async Task<IActionResult> Delete(string entityName, string id)
    {
        logger.LogInformation($"=> Delete: {entityName} - {id}");

        await entityEditor.DeleteItemAsync(entityName, id);

        return RedirectToAction("Index", new { entityName = entityName });
    }
}