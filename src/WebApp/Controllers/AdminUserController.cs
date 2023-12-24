using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]/[action]")]
public class AdminUserController : Controller
{
    private readonly UserManager<AppUser> _userManager;

    public AdminUserController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await Task.Run(() => _userManager.Users);
        return View(users);
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            ModelState.AddModelError("", "User not found");
            return View();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found");
            return View();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email")] AppUser model)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError("", "User not found");
            return View(model);
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }

            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            ModelState.AddModelError("", "User not found");
            return View();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found");
            return View();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, [Bind("Id,UserName,Email")] AppUser model)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError("", "User not found");
            return View(model);
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View(model);
    }
}