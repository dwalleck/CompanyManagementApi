using Microsoft.EntityFrameworkCore;
using Employee.Api.Types;
using Employee.Api.Extensions;

namespace Employee.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Types.Employee> Employees { get; set; }
    public DbSet<PayGroup> PayGroups { get; set; }
    public DbSet<PayEntry> PayEntries { get; set; }
    public DbSet<Disbursement> Disbursements { get; set; }
    public DbSet<BusinessEmployee> BusinessEmployees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure snake_case naming convention
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());
            
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }
        }

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

        modelBuilder.Entity<PayGroup>(entity =>
        {
            entity.ToTable("pay_groups");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .IsRequired();
        });

        modelBuilder.Entity<PayEntry>(entity =>
        {
            entity.ToTable("pay_entries");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .IsRequired();
            
            entity.Property(e => e.PayGroupId)
                .HasColumnName("pay_group_id");
            
            entity.Property(e => e.DisbursementId)
                .HasColumnName("disbursement_id");
            
            entity.Property(e => e.EmployeeId)
                .HasColumnName("employee_id")
                .HasMaxLength(36)
                .IsRequired();
            
            entity.Property(e => e.AccountNumber)
                .HasColumnName("account_number")
                .HasMaxLength(50)
                .IsRequired();
            
            entity.Property(e => e.RoutingNumber)
                .HasColumnName("routing_number")
                .HasMaxLength(20)
                .IsRequired();
            
            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();
            
            // Configure relationships with explicit property names to avoid conflicts
            entity.HasOne(pe => pe.PayGroup)
                .WithMany(pg => pg.PayEntries)
                .HasForeignKey(pe => pe.PayGroupId)
                .HasConstraintName("FK_PayEntries_PayGroups")
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(pe => pe.Disbursement)
                .WithMany(d => d.PayEntries)
                .HasForeignKey(pe => pe.DisbursementId)
                .HasConstraintName("FK_PayEntries_Disbursements")
                .OnDelete(DeleteBehavior.Cascade);
            
            // Add check constraint to ensure exclusivity
            entity.HasCheckConstraint(
                "CK_PayEntry_ExclusiveParent",
                @"(""type"" = 0 AND ""pay_group_id"" IS NOT NULL AND ""disbursement_id"" IS NULL) OR 
                  (""type"" = 1 AND ""pay_group_id"" IS NULL AND ""disbursement_id"" IS NOT NULL)"
            );
            
            // Add performance-critical indexes
            entity.HasIndex(e => e.Type)
                .HasDatabaseName("ix_pay_entries_type");
            
            entity.HasIndex(e => e.EmployeeId)
                .HasDatabaseName("ix_pay_entries_employee_id");
            
            entity.HasIndex(e => e.PayGroupId)
                .HasDatabaseName("ix_pay_entries_pay_group_id");
            
            entity.HasIndex(e => e.DisbursementId)
                .HasDatabaseName("ix_pay_entries_disbursement_id");
        });

        modelBuilder.Entity<Disbursement>(entity =>
        {
            entity.ToTable("disbursements");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            
            entity.Property(e => e.PayGroupId)
                .HasColumnName("pay_group_id")
                .IsRequired();
            
            entity.Property(e => e.DisbursementDate)
                .HasColumnName("disbursement_date")
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
            
            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .IsRequired();
            
            entity.Property(e => e.State)
                .HasColumnName("state")
                .IsRequired();
            
            // Configure relationship to PayGroup
            entity.HasOne(d => d.PayGroup)
                .WithMany(pg => pg.Disbursements)
                .HasForeignKey(d => d.PayGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Add performance indexes for common queries
            entity.HasIndex(e => e.State)
                .HasDatabaseName("ix_disbursements_state");
            
            entity.HasIndex(e => e.PayGroupId)
                .HasDatabaseName("ix_disbursements_pay_group_id");
            
            entity.HasIndex(e => e.DisbursementDate)
                .HasDatabaseName("ix_disbursements_disbursement_date");
        });

        modelBuilder.Entity<BusinessEmployee>(entity =>
        {
            entity.ToTable("business_employees");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();
            
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(320)
                .IsRequired();
            
            // Store bank accounts as JSON
            entity.Property(e => e.BankAccounts)
                .HasColumnName("bank_accounts")
                .HasColumnType("jsonb");
            
            // Add index for email lookups
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("ix_business_employees_email");
        });
    }
}