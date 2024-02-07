using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Models.Dto.AdminUserRole;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]/[action]")]
public class AdminUserRoleController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUserRoleController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        this._userManager = userManager;
        this._roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager
            .Users
            .ToListAsync();

        return View(users);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await GetUserByIdAsync(id);
        if (user is null)
        {
            return View();
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = await _roleManager.Roles.Select(s => s.Name).ToListAsync();

        var model = new UserRolesDto()
        {
            UserId = user.Id,
            UserName = user.UserName,
            UserRoles = userRoles,
            AllRoles = allRoles
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string userId, string addRole)
    {
        var user = await GetUserByIdAsync(userId);
        if (user is null || string.IsNullOrWhiteSpace(addRole))
        {
            return RedirectToAction(nameof(Edit), new { id = userId });
        }

        var addResult = await _userManager.AddToRoleAsync(user, addRole);

        if (!addResult.Succeeded)
        {
            foreach (var error in addResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return RedirectToAction(nameof(Edit), new { id = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string userId, string removeRole)
    {
        var user = await GetUserByIdAsync(userId);
        if (user is null || string.IsNullOrWhiteSpace(removeRole))
        {
            return RedirectToAction(nameof(Edit), new { id = userId });
        }

        var removeResult = await _userManager.RemoveFromRoleAsync(user, removeRole);

        if (!removeResult.Succeeded)
        {
            foreach (var error in removeResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return RedirectToAction(nameof(Edit), new { id = user.Id });
    }


    private async Task<AppUser?> GetUserByIdAsync(string userId)
    {
        if (userId is null)
        {
            ModelState.AddModelError("", "User not found");
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            ModelState.AddModelError("", "User not found");
        }

        return user;
    }
}