namespace PersonalFinanceMVC.Models.ViewModels;

public class LargeExpenseViewModel
{
    // ── 查詢條件 ──────────────────────────────────────────────
    public decimal MinAmount { get; set; } = 1000;
    public string StartMonth { get; set; } = DateTime.Now.AddMonths(-2).ToString("yyyy-MM");
    public string EndMonth   { get; set; } = DateTime.Now.ToString("yyyy-MM");

    // ── 狀態 ──────────────────────────────────────────────────
    public bool HasSearched { get; set; } = false;

    // ── 查詢結果 ──────────────────────────────────────────────
    public List<LargeExpenseMonthGroup> Groups { get; set; } = new();
    public decimal GrandTotal  => Groups.Sum(g => g.MonthTotal);
    public int     TotalCount  => Groups.Sum(g => g.Items.Count);
}

public class LargeExpenseMonthGroup
{
    public int    Year       { get; set; }
    public int    Month      { get; set; }
    public string MonthLabel => $"{Year}/{Month:D2}";
    public decimal MonthTotal { get; set; }
    public List<LargeExpenseRow> Items { get; set; } = new();
}

public class LargeExpenseRow
{
    public int      Id            { get; set; }
    public DateTime Date          { get; set; }
    public string   Description   { get; set; } = "";
    public string   CategoryName  { get; set; } = "";
    public string   CategoryColor { get; set; } = "#6c757d";
    public string   AccountName   { get; set; } = "";
    public decimal  Amount        { get; set; }
}
