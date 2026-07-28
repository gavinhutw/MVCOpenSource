using Microsoft.AspNetCore.Mvc;
using PersonalFinanceMVC.Models;
using PersonalFinanceMVC.Models.ViewModels;
using PersonalFinanceMVC.Services;

namespace PersonalFinanceMVC.Controllers;

public class AdvancePaymentController : Controller
{
    private readonly AdvancePaymentService _service;
    private readonly CategoryService _categoryService;

    public AdvancePaymentController(AdvancePaymentService service, CategoryService categoryService)
    {
        _service = service;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _service.GetAllAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new AdvancePaymentFormViewModel
        {
            Categories = await _categoryService.GetAllAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdvancePaymentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetAllAsync();
            return View(model);
        }

        var item = new AdvancePayment
        {
            Categories_Id = model.Categories_Id,
            Name          = model.Name,
            MonthCount    = model.MonthCount,
            Amount        = model.Amount
        };
        await _service.CreateAsync(item);
        TempData["Success"] = "預支項目新增成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return NotFound();

        var model = new AdvancePaymentFormViewModel
        {
            SerialNo      = item.SerialNo,
            Categories_Id = item.Categories_Id,
            Name          = item.Name,
            MonthCount    = item.MonthCount,
            Amount        = item.Amount,
            Categories    = await _categoryService.GetAllAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdvancePaymentFormViewModel model)
    {
        if (id != model.SerialNo) return BadRequest();

        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetAllAsync();
            return View(model);
        }

        var item = await _service.GetByIdAsync(id);
        if (item is null) return NotFound();

        item.Categories_Id = model.Categories_Id;
        item.Name          = model.Name;
        item.MonthCount    = model.MonthCount;
        item.Amount        = model.Amount;

        await _service.UpdateAsync(item);
        TempData["Success"] = "預支項目更新成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "預支項目已刪除";
        return RedirectToAction(nameof(Index));
    }
}
