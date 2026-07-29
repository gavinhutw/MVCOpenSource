using PersonalFinanceMVC.Models;

namespace PersonalFinanceMVC.Models.ViewModels;

public class ReverseExpenseViewModel
{
    // ── 查詢條件 ──────────────────────────────────────────────
    public int    CategoryId { get; set; }
    public string StartMonth { get; set; } = DateTime.Now.AddMonths(-2).ToString("yyyy-MM");
    public string EndMonth   { get; set; } = DateTime.Now.ToString("yyyy-MM");
    public string Keyword1   { get; set; } = "";
    public string Keyword2   { get; set; } = "";
    public string Keyword3   { get; set; } = "";
    public string Keyword4   { get; set; } = "";
    public string Keyword5   { get; set; } = "";

    // ── 狀態 ──────────────────────────────────────────────────
    public bool HasSearched { get; set; } = false;

    // ── 下拉選單資料 ──────────────────────────────────────────
    public List<Category> Categories { get; set; } = new();

    // ── 查詢結果 ──────────────────────────────────────────────
    public List<LargeExpenseMonthGroup> Groups { get; set; } = new();
    public decimal GrandTotal => Groups.Sum(g => g.MonthTotal);
    public int     TotalCount => Groups.Sum(g => g.Items.Count);

    // ── 已輸入的關鍵字（非空）──────────────────────────────────
    public List<string> ActiveKeywords =>
        new[] { Keyword1, Keyword2, Keyword3, Keyword4, Keyword5 }
        .Where(k => !string.IsNullOrWhiteSpace(k))
        .Select(k => k.Trim())
        .ToList();
}
