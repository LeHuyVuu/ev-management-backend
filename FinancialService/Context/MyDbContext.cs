using System;
using System.Collections.Generic;
using FinancialService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<contract> contracts { get; set; }

    public virtual DbSet<customer> customers { get; set; }

    public virtual DbSet<customer_debt> customer_debts { get; set; }

    public virtual DbSet<dealer> dealers { get; set; }

    public virtual DbSet<dealer_contract> dealer_contracts { get; set; }

    public virtual DbSet<dealer_discount> dealer_discounts { get; set; }

    public virtual DbSet<dealer_target> dealer_targets { get; set; }

    public virtual DbSet<inventory> inventories { get; set; }

    public virtual DbSet<manufacturer_debt> manufacturer_debts { get; set; }

    public virtual DbSet<order> orders { get; set; }

    public virtual DbSet<permission> permissions { get; set; }

    public virtual DbSet<promotion> promotions { get; set; }

    public virtual DbSet<quote> quotes { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<test_drife> test_drives { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<vehicle> vehicles { get; set; }

    public virtual DbSet<vehicle_allocation> vehicle_allocations { get; set; }

    public virtual DbSet<vehicle_version> vehicle_versions { get; set; }

    public virtual DbSet<wholesale_price> wholesale_prices { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<contract>(entity =>
        {
            entity.HasKey(e => e.contract_id).HasName("contracts_pkey");

            entity.Property(e => e.contract_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.customer).WithMany(p => p.contracts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contracts_customer_id_fkey");

            entity.HasOne(d => d.dealer).WithMany(p => p.contracts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contracts_dealer_id_fkey");

            entity.HasOne(d => d.quote).WithMany(p => p.contracts).HasConstraintName("contracts_quote_id_fkey");
        });

        modelBuilder.Entity<customer>(entity =>
        {
            entity.HasKey(e => e.customer_id).HasName("customers_pkey");

            entity.Property(e => e.customer_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.dealer).WithMany(p => p.customers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customers_dealer_id_fkey");
        });

        modelBuilder.Entity<customer_debt>(entity =>
        {
            entity.HasKey(e => e.customer_debt_id).HasName("customer_debts_pkey");

            entity.Property(e => e.customer_debt_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.customer).WithMany(p => p.customer_debts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_debts_customer_id_fkey");

            entity.HasOne(d => d.dealer).WithMany(p => p.customer_debts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_debts_dealer_id_fkey");
        });

        modelBuilder.Entity<dealer>(entity =>
        {
            entity.HasKey(e => e.dealer_id).HasName("dealers_pkey");

            entity.Property(e => e.dealer_id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<dealer_contract>(entity =>
        {
            entity.HasKey(e => e.dealer_contract_id).HasName("dealer_contracts_pkey");

            entity.Property(e => e.dealer_contract_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.dealer).WithMany(p => p.dealer_contracts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dealer_contracts_dealer_id_fkey");
        });

        modelBuilder.Entity<dealer_discount>(entity =>
        {
            entity.HasKey(e => e.dealer_discount_id).HasName("dealer_discounts_pkey");

            entity.Property(e => e.dealer_discount_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.dealer).WithMany(p => p.dealer_discounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dealer_discounts_dealer_id_fkey");
        });

        modelBuilder.Entity<dealer_target>(entity =>
        {
            entity.HasKey(e => e.dealer_target_id).HasName("dealer_targets_pkey");

            entity.Property(e => e.dealer_target_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.dealer).WithMany(p => p.dealer_targets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dealer_targets_dealer_id_fkey");
        });

        modelBuilder.Entity<inventory>(entity =>
        {
            entity.HasKey(e => e.inventory_id).HasName("inventory_pkey");

            entity.Property(e => e.inventory_id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.last_updated).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.stock_quantity).HasDefaultValue(0);

            entity.HasOne(d => d.dealer).WithMany(p => p.inventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inventory_dealer_id_fkey");

            entity.HasOne(d => d.vehicle_version).WithMany(p => p.inventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inventory_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<manufacturer_debt>(entity =>
        {
            entity.HasKey(e => e.manufacturer_debt_id).HasName("manufacturer_debts_pkey");

            entity.Property(e => e.manufacturer_debt_id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<order>(entity =>
        {
            entity.HasKey(e => e.order_id).HasName("orders_pkey");

            entity.Property(e => e.order_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.contract).WithMany(p => p.orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_contract_id_fkey");

            entity.HasOne(d => d.customer).WithMany(p => p.orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_customer_id_fkey");

            entity.HasOne(d => d.dealer).WithMany(p => p.orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_dealer_id_fkey");
        });

        modelBuilder.Entity<permission>(entity =>
        {
            entity.HasKey(e => e.permission_id).HasName("permissions_pkey");
        });

        modelBuilder.Entity<promotion>(entity =>
        {
            entity.HasKey(e => e.promotion_id).HasName("promotions_pkey");

            entity.Property(e => e.promotion_id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<quote>(entity =>
        {
            entity.HasKey(e => e.quote_id).HasName("quotes_pkey");

            entity.Property(e => e.quote_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.created_by_userNavigation).WithMany(p => p.quotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotes_created_by_user_fkey");

            entity.HasOne(d => d.customer).WithMany(p => p.quotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotes_customer_id_fkey");

            entity.HasOne(d => d.dealer).WithMany(p => p.quotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotes_dealer_id_fkey");

            entity.HasOne(d => d.vehicle_version).WithMany(p => p.quotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotes_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.role_id).HasName("roles_pkey");

            entity.HasMany(d => d.permissions).WithMany(p => p.roles)
                .UsingEntity<Dictionary<string, object>>(
                    "role_permission",
                    r => r.HasOne<permission>().WithMany()
                        .HasForeignKey("permission_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("role_permissions_permission_id_fkey"),
                    l => l.HasOne<role>().WithMany()
                        .HasForeignKey("role_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("role_permissions_role_id_fkey"),
                    j =>
                    {
                        j.HasKey("role_id", "permission_id").HasName("role_permissions_pkey");
                        j.ToTable("role_permissions", "evm");
                    });
        });

        modelBuilder.Entity<test_drife>(entity =>
        {
            entity.HasKey(e => e.test_drive_id).HasName("test_drives_pkey");

            entity.Property(e => e.test_drive_id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.confirm_email).HasDefaultValue(false);
            entity.Property(e => e.confirm_sms).HasDefaultValue(false);

            entity.HasOne(d => d.customer).WithMany(p => p.test_drives)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("test_drives_customer_id_fkey");

            entity.HasOne(d => d.dealer).WithMany(p => p.test_drives)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("test_drives_dealer_id_fkey");

            entity.HasOne(d => d.staff_user).WithMany(p => p.test_drives).HasConstraintName("test_drives_staff_user_id_fkey");

            entity.HasOne(d => d.vehicle_version).WithMany(p => p.test_drives)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("test_drives_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("users_pkey");

            entity.Property(e => e.user_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.dealer).WithMany(p => p.users).HasConstraintName("users_dealer_id_fkey");

            entity.HasOne(d => d.role).WithMany(p => p.users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_role_id_fkey");
        });

        modelBuilder.Entity<vehicle>(entity =>
        {
            entity.HasKey(e => e.vehicle_id).HasName("vehicles_pkey");

            entity.Property(e => e.vehicle_id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<vehicle_allocation>(entity =>
        {
            entity.HasKey(e => e.allocation_id).HasName("vehicle_allocations_pkey");

            entity.Property(e => e.allocation_id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.request_date).HasDefaultValueSql("CURRENT_DATE");

            entity.HasOne(d => d.dealer).WithMany(p => p.vehicle_allocations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vehicle_allocations_dealer_id_fkey");

            entity.HasOne(d => d.vehicle_version).WithMany(p => p.vehicle_allocations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vehicle_allocations_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<vehicle_version>(entity =>
        {
            entity.HasKey(e => e.vehicle_version_id).HasName("vehicle_versions_pkey");

            entity.Property(e => e.vehicle_version_id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.vehicle).WithMany(p => p.vehicle_versions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vehicle_versions_vehicle_id_fkey");
        });

        modelBuilder.Entity<wholesale_price>(entity =>
        {
            entity.HasKey(e => e.wholesale_price_id).HasName("wholesale_prices_pkey");

            entity.Property(e => e.wholesale_price_id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.min_order_quantity).HasDefaultValue(1);

            entity.HasOne(d => d.vehicle_version).WithMany(p => p.wholesale_prices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("wholesale_prices_vehicle_version_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
