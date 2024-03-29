﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Interfaces;
using WebApp.Models;

namespace WebApp.Controllers;


public class HomeController : Controller
{
    private readonly IEntityStorage _repos;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IEntityStorage repos, ILogger<HomeController> logger)
    {
        _repos = repos;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
