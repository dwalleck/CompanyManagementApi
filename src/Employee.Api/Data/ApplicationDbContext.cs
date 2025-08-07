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
    public DbSet<PayGroup> PayGroups { get; set; }
    public DbSet<PayEntry> PayEntries { get; set; }
    public DbSet<Disbursement> Disbursements { get; set; }
    public DbSet<BusinessEmployee> BusinessEmployees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureEmployee(modelBuilder);
        ConfigurePayGroup(modelBuilder);
        ConfigurePayEntry(modelBuilder);
        ConfigureDisbursement(modelBuilder);
        ConfigureBusinessEmployee(modelBuilder);
    }
    
    private static void ConfigureEmployee(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Types.Employee>(entity =>
        {
            entity.Property(e => e.EmployeeId).HasMaxLength(36);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Department).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Salary).HasPrecision(18, 2);
            
            entity.HasIndex(e => e.Department);
        });
    }
    
    private static void ConfigurePayGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PayGroup>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });
    }
    
    private static void ConfigurePayEntry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PayEntry>(entity =>
        {
            entity.Property(e => e.EmployeeId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.AccountNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RoutingNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            
            // Relationships
            entity.HasOne(pe => pe.PayGroup)
                .WithMany(pg => pg.PayEntries)
                .HasForeignKey(pe => pe.PayGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(pe => pe.Disbursement)
                .WithMany(d => d.PayEntries)
                .HasForeignKey(pe => pe.DisbursementId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Exclusivity constraint - ensures PayEntry belongs to either PayGroup OR Disbursement
            entity.HasCheckConstraint(
                "CK_PayEntry_ExclusiveParent",
                @"(type = 0 AND pay_group_id IS NOT NULL AND disbursement_id IS NULL) OR 
                  (type = 1 AND pay_group_id IS NULL AND disbursement_id IS NOT NULL)"
            );
            
            // Performance indexes
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.PayGroupId);
            entity.HasIndex(e => e.DisbursementId);
        });
    }
    
    private static void ConfigureDisbursement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Disbursement>(entity =>
        {
            // Relationship
            entity.HasOne(d => d.PayGroup)
                .WithMany(pg => pg.Disbursements)
                .HasForeignKey(d => d.PayGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Performance indexes
            entity.HasIndex(e => e.State);
            entity.HasIndex(e => e.PayGroupId);
            entity.HasIndex(e => e.DisbursementDate);
        });
    }
    
    private static void ConfigureBusinessEmployee(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessEmployee>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(320).IsRequired();
            
            // Store bank accounts as JSON in PostgreSQL
            entity.Property(e => e.BankAccounts).HasColumnType("jsonb");
            
            // Unique constraint on email
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

// In Program.cs or Startup.cs, configure like this:
/*
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());
*/