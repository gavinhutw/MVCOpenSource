using System.ComponentModel.DataAnnotations;

namespace EnterprisePortal.Models
{
    public class Role
    {
        [Key]
        [StringLength(20)]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "角色名稱")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "角色描述")]
        public string? Description { get; set; }

        public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    }

    public class EmployeeRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RoleId { get; set; } = string.Empty;

        public virtual Employee? Employee { get; set; }
        public virtual Role? Role { get; set; }
    }
}
