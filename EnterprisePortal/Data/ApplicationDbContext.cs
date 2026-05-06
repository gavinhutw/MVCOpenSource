using EnterprisePortal.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EnterprisePortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "EnterprisePortalSalt"));
            return Convert.ToBase64String(bytes);
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<LeaveApprover> LeaveApprovers { get; set; }
        public DbSet<OvertimeApplication> OvertimeApplications { get; set; }
        public DbSet<OvertimeApprover> OvertimeApprovers { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee self-referencing supervisor relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Supervisor)
                .WithMany()
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.Applicant)
                .WithMany(e => e.LeaveApplications)
                .HasForeignKey(l => l.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.Proxy)
                .WithMany()
                .HasForeignKey(l => l.ProxyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveApprover>()
                .HasOne(la => la.LeaveApplication)
                .WithMany(l => l.Approvers)
                .HasForeignKey(la => la.LeaveApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveApprover>()
                .HasOne(la => la.Approver)
                .WithMany()
                .HasForeignKey(la => la.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OvertimeApplication>()
                .HasOne(o => o.Applicant)
                .WithMany(e => e.OvertimeApplications)
                .HasForeignKey(o => o.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OvertimeApplication>()
                .HasOne(o => o.Proxy)
                .WithMany()
                .HasForeignKey(o => o.ProxyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OvertimeApprover>()
                .HasOne(oa => oa.OvertimeApplication)
                .WithMany(o => o.Approvers)
                .HasForeignKey(oa => oa.OvertimeApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OvertimeApprover>()
                .HasOne(oa => oa.Approver)
                .WithMany()
                .HasForeignKey(oa => oa.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttendanceRecord>()
                .Property(a => a.Temperature)
                .HasColumnType("decimal(4,1)");

            // EmployeeRole relationships
            modelBuilder.Entity<EmployeeRole>()
                .HasOne(er => er.Employee)
                .WithMany()
                .HasForeignKey(er => er.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeRole>()
                .HasOne(er => er.Role)
                .WithMany(r => r.EmployeeRoles)
                .HasForeignKey(er => er.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeRole>()
                .HasIndex(er => new { er.EmployeeId, er.RoleId })
                .IsUnique();

            // Seed departments
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = "DEPT001", DepartmentName = "總經理室" },
                new Department { DepartmentId = "DEPT002", DepartmentName = "人力資源部" },
                new Department { DepartmentId = "DEPT003", DepartmentName = "財務部" },
                new Department { DepartmentId = "DEPT004", DepartmentName = "資訊部" },
                new Department { DepartmentId = "DEPT005", DepartmentName = "業務部" },
                new Department { DepartmentId = "DEPT006", DepartmentName = "行政部" }
            );

            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = "ADMIN", RoleName = "系統管理員", Description = "擁有所有功能權限，可管理員工資料及角色指派" },
                new Role { RoleId = "MANAGER", RoleName = "主管", Description = "可審核下屬請假/加班申請，查看部門報表" },
                new Role { RoleId = "EMPLOYEE", RoleName = "一般員工", Description = "可使用請假、加班、刷卡等基本功能" }
            );

            // Seed admin employee (password: Admin@123)
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    EmployeeId = "EMP001",
                    FullName = "系統管理員",
                    EnglishName = "Admin",
                    Gender = GenderType.Male,
                    MaritalStatus = MaritalStatus.Single,
                    EducationLevel = EducationLevel.Bachelor,
                    HireDate = new DateTime(2020, 1, 1),
                    EmploymentStatus = EmploymentStatus.Active,
                    DepartmentId = "DEPT001",
                    JobTitle = "系統管理員",
                    PasswordHash = HashPassword("Admin@123")
                }
            );

            // Assign admin role to EMP001
            modelBuilder.Entity<EmployeeRole>().HasData(
                new EmployeeRole { Id = 1, EmployeeId = "EMP001", RoleId = "ADMIN" },
                new EmployeeRole { Id = 2, EmployeeId = "EMP001", RoleId = "EMPLOYEE" }
            );
        }
    }
}
