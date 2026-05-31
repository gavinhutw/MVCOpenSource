using Microsoft.AspNetCore.Mvc;
using PersonalFinanceMVC.Models;
using PersonalFinanceMVC.Services;

namespace PersonalFinanceMVC.Controllers;

public class CategoriesController : Controller
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService) => _categoryService = categoryService;

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();
        return View(categories);
    }

    public IActionResult Create() => View(new Category());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid) return View(category);
        await _categoryService.CreateAsync(category);
        TempData["Success"] = "分類新增成功";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return BadRequest();
        if (!ModelState.IsValid) return View(category);
        await _categoryService.UpdateAsync(category);
        TempData["Success"] = "分類更新成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        TempData["Success"] = "分類已刪除";
        return RedirectToAction(nameof(Index));
    }
}
