using Microsoft.EntityFrameworkCore;
using Employee.Api.Types;

namespace Employee.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Types.Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Types.Employee>(entity =>
        {
            entity.ToTable("employees");
            
            entity.HasKey(e => e.EmployeeId);
            
            entity.Property(e => e.EmployeeId)
                .HasColumnName("employee_id")
                .HasMaxLength(36)
                .IsRequired();
            
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(e => e.Department)
                .HasColumnName("department")
                .HasMaxLength(50)
                .IsRequired();
            
            entity.Property(e => e.Salary)
                .HasColumnName("salary")
                .HasPrecision(18, 2);
            
            entity.Property(e => e.HireDate)
                .HasColumnName("hire_date");
            
            entity.Property(e => e.LastModified)
                .HasColumnName("last_modified");
            
            entity.HasIndex(e => e.Department)
                .HasDatabaseName("ix_employees_department");
        });
    }
}