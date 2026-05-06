using System.ComponentModel.DataAnnotations;
using EnterprisePortal.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnterprisePortal.ViewModels
{
    public class AttendanceViewModel
    {
        [Required(ErrorMessage = "請選擇刷卡類別")]
        [Display(Name = "刷卡類別")]
        public ClockType ClockType { get; set; }

        [Required(ErrorMessage = "請輸入體溫")]
        [Display(Name = "體溫 (℃)")]
        [Range(35.0, 42.0, ErrorMessage = "體溫須介於 35.0℃ 至 42.0℃ 之間")]
        public decimal Temperature { get; set; }

        public SelectList? ClockTypeList { get; set; }
    }
}
