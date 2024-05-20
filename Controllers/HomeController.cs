using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using App.Models;
using Microsoft.EntityFrameworkCore;
using App.Data;
using Microsoft.AspNetCore.Identity;

namespace App.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;


    public HomeController(ILogger<HomeController> logger, AppDbContext context, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> IndexAsync()
    {
        // tao ra cac roles duoc dinh nghia san trong Data\RoleNames.cs
        var roles = typeof(RoleName).GetFields().ToList();
        foreach (var role in roles)
        {
            var roleName = (string)role.GetRawConstantValue();
            var isHasRole = await _roleManager.FindByNameAsync(roleName);
            if(isHasRole == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var products = _context.Products.Include(p => p.Author)
                                        .Include(p => p.Photos)
                                        .Include(p => p.ProductCategoryProducts)
                                        .ThenInclude(pc => pc.CategoryProduct)
                                        .AsQueryable(); 
        products = products.OrderByDescending(p => p.DateUpdated).Take(4);

        var posts = _context.Posts.Include(p => p.Author)
                                        .Include(p => p.PostCategories)
                                        .ThenInclude(pc => pc.CategoryModel)
                                        .AsQueryable();   
        posts = posts.OrderByDescending(p => p.DateUpdated).Take(5);

        ViewBag.products = products;
        ViewBag.posts = posts;

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
