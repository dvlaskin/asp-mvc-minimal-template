using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        ViewData["ReturnUrl"] = $"{nameof(Index)}/{entityName}";

        ViewBag.TablesList = entityEditor.GetTablesList();

        var model = entityEditor.GetDefaultNewRecordModel(entityName);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string entityName, [FromForm] Dictionary<string, string> formData)
    {
        await entityEditor.CreateEntityRecordAsync(entityName, formData);

        return RedirectToAction("Index", new { entityName = entityName });
    }


    [HttpGet("{entityName}/{id}")]
    public async Task<IActionResult> Edit(string entityName, string id)
    {
        // url for return from edit page
        var currentUrl = Request.Path.Value;
        var prefixUrl = currentUrl?[..currentUrl.IndexOf(nameof(Edit))];
        ViewData["ReturnUrl"] = $"{prefixUrl}{nameof(Index)}/{entityName}";

        ViewBag.TablesList = entityEditor.GetTablesList();

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
        await entityEditor.UpdateRecordAsync(entityName, formData);

        return RedirectToAction("Index", new { entityName = entityName });
    }


    [HttpGet("{entityName}/{id}")]
    public async Task<IActionResult> Delete(string entityName, string id)
    {
        await entityEditor.DeleteItemAsync(entityName, id);

        return RedirectToAction("Index", new { entityName = entityName });
    }
}