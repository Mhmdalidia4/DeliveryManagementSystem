using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace App.Domain.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<DriverAssignment> DriverAssignments { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderArea> OrderAreas { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<OrdersToReview> OrdersToReviews { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PickUpRequest> PickupRequests { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=Mohamad_Ali_Dia\\SQLEXPRESS;Database=Delivery_System;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Company__2D971CAC5B915FF5");

            entity.ToTable("Company");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.SubscriptionType).HasMaxLength(100);
            entity.Property(e => e.UserId).HasMaxLength(450);
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.DriverId).HasName("PK__Driver__F1B1CD04A0A4EEFD");

            entity.ToTable("Driver");

            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Company).WithMany(p => p.Drivers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Driver__CompanyI__5165187F");
        });

        modelBuilder.Entity<DriverAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__DriverAs__32499E77A1A48BE3");

            entity.ToTable("DriverAssignment");

            entity.Property(e => e.AssignedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Driver).WithMany(p => p.DriverAssignments)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DriverAss__Drive__6477ECF3");

            entity.HasOne(d => d.Order).WithMany(p => p.DriverAssignments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DriverAss__Order__6383C8BA");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD68F1D10AC");

            entity.ToTable("Feedback");

            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.SubmittedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__OrderI__6754599E");
        });        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1228D80A5F");

            entity.ToTable("Notification");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(450);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BCF9A02C0D1");

            entity.ToTable("Order");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CustomerAddress).HasMaxLength(255);
            entity.Property(e => e.CustomerName).HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.EstimatedArrivalTime).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.TrackingPassword).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__CompanyId__5CD6CB2B");

            entity.HasOne(d => d.OrderArea).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderAreaId)
                .HasConstraintName("FK__Order__OrderArea__5EBF139D");

            entity.HasOne(d => d.OrderStatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderStatusId)
                .HasConstraintName("FK__Order__OrderStat__5FB337D6");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("FK__Order__PaymentMe__60A75C0F");

            entity.HasOne(d => d.Shop).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("FK__Order__ShopId__5DCAEF64");
        });

        modelBuilder.Entity<OrderArea>(entity =>
        {
            entity.HasKey(e => e.OrderAreaId).HasName("PK__OrderAre__BA78C3F6B86AA73A");

            entity.ToTable("OrderArea");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.OrderAreas)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderArea__Compa__5441852A");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.OrderStatusId).HasName("PK__OrderSta__BC674CA1B87E8CD2");

            entity.ToTable("OrderStatus");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.OrderStatuses)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderStat__Compa__571DF1D5");
        });

        modelBuilder.Entity<OrdersToReview>(entity =>
        {
            entity.HasKey(e => e.OrderToReviewId).HasName("PK__OrdersTo__F078B8DCD81527F3");

            entity.ToTable("OrdersToReview");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CustomerAddress).HasMaxLength(255);
            entity.Property(e => e.CustomerName).HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Status).HasColumnType("bit");
            entity.Property(e => e.TrackingPassword).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.OrdersToReviews)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrdersToR__Compa__6C190EBB");

            entity.HasOne(d => d.OrderArea).WithMany(p => p.OrdersToReviews)
                .HasForeignKey(d => d.OrderAreaId)
                .HasConstraintName("FK__OrdersToR__Order__6E01572D");

            entity.HasOne(d => d.Shop).WithMany(p => p.OrdersToReviews)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrdersToR__ShopI__6D0D32F4");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1D33EC53D08");

            entity.ToTable("PaymentMethod");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.PaymentMethods)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaymentMe__Compa__59FA5E80");
        });

        modelBuilder.Entity<PickUpRequest>(entity =>
        {
            entity.HasKey(e => e.PickupRequestId).HasName("PK__PickupRe__898E6238E46BB8F4");

            entity.ToTable("PickupRequest");

            entity.Property(e => e.RequestedAt).HasColumnType("datetime");

            entity.HasOne(d => d.AssignedDriver).WithMany(p => p.PickupRequests)
                .HasForeignKey(d => d.AssignedDriverId)
                .HasConstraintName("FK__PickupReq__Assig__72C60C4A");

            entity.HasOne(d => d.Company).WithMany(p => p.PickupRequests)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PickupReq__Compa__70DDC3D8");

            entity.HasOne(d => d.Shop).WithMany(p => p.PickupRequests)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PickupReq__ShopI__71D1E811");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PK__Shop__67C557C9532DCD68");

            entity.ToTable("Shop");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Company).WithMany(p => p.Shops)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Shop__CompanyId__4E88ABD4");
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__SystemLo__5E548648E2725D02");

            entity.ToTable("SystemLog");

            entity.Property(e => e.Level).HasMaxLength(32);
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
