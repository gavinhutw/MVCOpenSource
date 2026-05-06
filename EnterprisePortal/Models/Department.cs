using System.ComponentModel.DataAnnotations;

namespace EnterprisePortal.Models
{
    public class Department
    {
        [Key]
        [StringLength(20)]
        public string DepartmentId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "部門名稱")]
        public string DepartmentName { get; set; } = string.Empty;

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
