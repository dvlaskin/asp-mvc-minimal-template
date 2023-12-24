using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Db;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]/[action]")]
public class AdminEntityController : Controller
{
    private readonly AppDbContext appDbContext;

    public AdminEntityController(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task<IActionResult> Index()
    {
        var dbSetProperties = typeof(AppDbContext)
            .GetProperties()
            .Where(p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        if (dbSetProperties != null)
        {
            var propertiesNames = dbSetProperties.Select(s => s.Name).ToArray();
            return View(propertiesNames);
        }

        // Handle error case when the specified entity is not found
        return View("EntityNotFound");
    }
}