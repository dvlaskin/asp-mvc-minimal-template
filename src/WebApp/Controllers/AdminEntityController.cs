using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Interfaces;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]/[action]")]
public class AdminEntityController : Controller
{
    private readonly IAdminEntityEditor _entityEditor;
    private readonly ILogger<AdminEntityController> _logger;

    public AdminEntityController(
        IAdminEntityEditor entityEditor,
        ILogger<AdminEntityController> logger)
    {
        this._entityEditor = entityEditor;
        this._logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.TablesList = _entityEditor.GetTablesList();
        return View();
    }

    [HttpGet("{entityName}")]
    public async Task<IActionResult> Index(string entityName)
    {
        ViewBag.TablesList = _entityEditor.GetTablesList();

        var entityRecords = await _entityEditor.GetAllRecordFromDbSetAsync(entityName);

        return View(entityRecords);
    }


    [HttpGet]
    public IActionResult Create(string entityName)
    {
        ViewData["ReturnUrl"] = $"{nameof(Index)}/{entityName}";

        ViewBag.TablesList = _entityEditor.GetTablesList();

        var model = _entityEditor.GetDefaultNewRecordModel(entityName);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string entityName, [FromForm] Dictionary<string, string> formData)
    {
        await _entityEditor.CreateEntityRecordAsync(entityName, formData);

        return RedirectToAction("Index", new { entityName = entityName });
    }


    [HttpGet("{entityName}/{id}")]
    public async Task<IActionResult> Edit(string entityName, string id)
    {
        // url for return from edit page
        var currentUrl = Request.Path.Value;
        var prefixUrl = currentUrl?[..currentUrl.IndexOf(nameof(Edit))];
        ViewData["ReturnUrl"] = $"{prefixUrl}{nameof(Index)}/{entityName}";

        ViewBag.TablesList = _entityEditor.GetTablesList();

        var entityRecord = await _entityEditor.GetRecordValuesAsync(entityName, id);
        if (entityRecord is null)
        {
            return NotFound();
        }

        return View(entityRecord);
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] string entityName, [FromForm] Dictionary<string, string> formData)
    {
        await _entityEditor.UpdateRecordAsync(entityName, formData);

        return RedirectToAction("Index", new { entityName = entityName });
    }


    [HttpGet("{entityName}/{id}")]
    public async Task<IActionResult> Delete(string entityName, string id)
    {
        await _entityEditor.DeleteItemAsync(entityName, id);

        return RedirectToAction("Index", new { entityName = entityName });
    }
}