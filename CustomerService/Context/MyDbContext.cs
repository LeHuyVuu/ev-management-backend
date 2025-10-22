using System;
using System.Collections.Generic;
using CustomerService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BrandInventory> BrandInventories { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerDebt> CustomerDebts { get; set; }

    public virtual DbSet<Dealer> Dealers { get; set; }

    public virtual DbSet<DealerContract> DealerContracts { get; set; }

    public virtual DbSet<DealerDiscount> DealerDiscounts { get; set; }

    public virtual DbSet<DealerTarget> DealerTargets { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<ManufacturerDebt> ManufacturerDebts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Quote> Quotes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TestDrife> TestDrives { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleAllocation> VehicleAllocations { get; set; }

    public virtual DbSet<VehicleTransferOrder> VehicleTransferOrders { get; set; }

    public virtual DbSet<VehicleVersion> VehicleVersions { get; set; }

    public virtual DbSet<WholesalePrice> WholesalePrices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BrandInventory>(entity =>
        {
            entity.HasKey(e => e.VehicleVersionId).HasName("brand_inventory_pkey");

            entity.ToTable("brand_inventory", "evm");

            entity.Property(e => e.VehicleVersionId)
                .ValueGeneratedNever()
                .HasColumnName("vehicle_version_id");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");

            entity.HasOne(d => d.VehicleVersion).WithOne(p => p.BrandInventory)
                .HasForeignKey<BrandInventory>(d => d.VehicleVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("brand_inventory_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("contracts_pkey");

            entity.ToTable("contracts", "evm");

            entity.Property(e => e.ContractId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("contract_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .HasColumnName("payment_method");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
            entity.Property(e => e.SignedDate).HasColumnName("signed_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.TotalValue)
                .HasPrecision(18, 2)
                .HasColumnName("total_value");

            entity.HasOne(d => d.Customer).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contracts_customer_id_fkey");

            entity.HasOne(d => d.Dealer).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contracts_dealer_id_fkey");

            entity.HasOne(d => d.Quote).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.QuoteId)
                .HasConstraintName("contracts_quote_id_fkey");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customers_pkey");

            entity.ToTable("customers", "evm");

            entity.HasIndex(e => e.DealerId, "idx_customers_dealer");

            entity.Property(e => e.CustomerId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("customer_id");
            entity.Property(e => e.Address)
                .HasMaxLength(300)
                .HasColumnName("address");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.LastInteractionDate).HasColumnName("last_interaction_date");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");

            entity.HasOne(d => d.Dealer).WithMany(p => p.Customers)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customers_dealer_id_fkey");
        });

        modelBuilder.Entity<CustomerDebt>(entity =>
        {
            entity.HasKey(e => e.CustomerDebtId).HasName("customer_debts_pkey");

            entity.ToTable("customer_debts", "evm");

            entity.Property(e => e.CustomerDebtId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("customer_debt_id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerDebts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_debts_customer_id_fkey");

            entity.HasOne(d => d.Dealer).WithMany(p => p.CustomerDebts)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_debts_dealer_id_fkey");
        });

        modelBuilder.Entity<Dealer>(entity =>
        {
            entity.HasKey(e => e.DealerId).HasName("dealers_pkey");

            entity.ToTable("dealers", "evm");

            entity.HasIndex(e => e.DealerCode, "dealers_dealer_code_key").IsUnique();

            entity.Property(e => e.DealerId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("dealer_id");
            entity.Property(e => e.Address)
                .HasMaxLength(300)
                .HasColumnName("address");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(200)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(50)
                .HasColumnName("contact_phone");
            entity.Property(e => e.DealerCode)
                .HasMaxLength(50)
                .HasColumnName("dealer_code");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Region)
                .HasMaxLength(100)
                .HasColumnName("region");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
        });

        modelBuilder.Entity<DealerContract>(entity =>
        {
            entity.HasKey(e => e.DealerContractId).HasName("dealer_contracts_pkey");

            entity.ToTable("dealer_contracts", "evm");

            entity.Property(e => e.DealerContractId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("dealer_contract_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");

            entity.HasOne(d => d.Dealer).WithMany(p => p.DealerContracts)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dealer_contracts_dealer_id_fkey");
        });

        modelBuilder.Entity<DealerDiscount>(entity =>
        {
            entity.HasKey(e => e.DealerDiscountId).HasName("dealer_discounts_pkey");

            entity.ToTable("dealer_discounts", "evm");

            entity.Property(e => e.DealerDiscountId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("dealer_discount_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.DiscountRate)
                .HasPrecision(5, 2)
                .HasColumnName("discount_rate");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");

            entity.HasOne(d => d.Dealer).WithMany(p => p.DealerDiscounts)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dealer_discounts_dealer_id_fkey");
        });

        modelBuilder.Entity<DealerTarget>(entity =>
        {
            entity.HasKey(e => e.DealerTargetId).HasName("dealer_targets_pkey");

            entity.ToTable("dealer_targets", "evm");

            entity.Property(e => e.DealerTargetId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("dealer_target_id");
            entity.Property(e => e.AchievedAmount)
                .HasPrecision(18, 2)
                .HasColumnName("achieved_amount");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.Period)
                .HasMaxLength(30)
                .HasColumnName("period");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.TargetAmount)
                .HasPrecision(18, 2)
                .HasColumnName("target_amount");

            entity.HasOne(d => d.Dealer).WithMany(p => p.DealerTargets)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dealer_targets_dealer_id_fkey");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("inventory_pkey");

            entity.ToTable("inventory", "evm");

            entity.HasIndex(e => new { e.DealerId, e.VehicleVersionId }, "inventory_dealer_id_vehicle_version_id_key").IsUnique();

            entity.Property(e => e.InventoryId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("inventory_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("last_updated");
            entity.Property(e => e.StockQuantity)
                .HasDefaultValue(0)
                .HasColumnName("stock_quantity");
            entity.Property(e => e.VehicleVersionId).HasColumnName("vehicle_version_id");

            entity.HasOne(d => d.Dealer).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inventory_dealer_id_fkey");

            entity.HasOne(d => d.VehicleVersion).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.VehicleVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inventory_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<ManufacturerDebt>(entity =>
        {
            entity.HasKey(e => e.ManufacturerDebtId).HasName("manufacturer_debts_pkey");

            entity.ToTable("manufacturer_debts", "evm");

            entity.Property(e => e.ManufacturerDebtId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("manufacturer_debt_id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.ManufacturerName)
                .HasMaxLength(200)
                .HasColumnName("manufacturer_name");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("orders_pkey");

            entity.ToTable("orders", "evm");

            entity.Property(e => e.OrderId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("order_id");
            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.DeliveryAddress)
                .HasMaxLength(300)
                .HasColumnName("delivery_address");
            entity.Property(e => e.DeliveryDate).HasColumnName("delivery_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");

            entity.HasOne(d => d.Contract).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_contract_id_fkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_customer_id_fkey");

            entity.HasOne(d => d.Dealer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_dealer_id_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("permissions_pkey");

            entity.ToTable("permissions", "evm");

            entity.HasIndex(e => e.PermissionKey, "permissions_permission_key_key").IsUnique();

            entity.Property(e => e.PermissionId)
                .HasDefaultValueSql("nextval('permissions_permission_id_seq'::regclass)")
                .HasColumnName("permission_id");
            entity.Property(e => e.PermissionKey)
                .HasMaxLength(100)
                .HasColumnName("permission_key");
            entity.Property(e => e.PermissionName)
                .HasMaxLength(200)
                .HasColumnName("permission_name");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("promotions_pkey");

            entity.ToTable("promotions", "evm");

            entity.Property(e => e.PromotionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("promotion_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.QuoteId).HasName("quotes_pkey");

            entity.ToTable("quotes", "evm");

            entity.HasIndex(e => e.DealerId, "idx_quotes_dealer");

            entity.Property(e => e.QuoteId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("quote_id");
            entity.Property(e => e.CreatedByUser).HasColumnName("created_by_user");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.DiscountAmt)
                .HasPrecision(18, 2)
                .HasColumnName("discount_amt");
            entity.Property(e => e.DiscountCode)
                .HasMaxLength(100)
                .HasColumnName("discount_code");
            entity.Property(e => e.OptionsJson).HasColumnName("options_json");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.Subtotal)
                .HasPrecision(18, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(18, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.VehicleVersionId).HasColumnName("vehicle_version_id");

            entity.HasOne(d => d.CreatedByUserNavigation).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.CreatedByUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotes_created_by_user_fkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotes_customer_id_fkey");

            entity.HasOne(d => d.Dealer).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotes_dealer_id_fkey");

            entity.HasOne(d => d.VehicleVersion).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.VehicleVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotes_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("roles_pkey");

            entity.ToTable("roles", "evm");

            entity.HasIndex(e => e.RoleKey, "roles_role_key_key").IsUnique();

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("nextval('roles_role_id_seq'::regclass)")
                .HasColumnName("role_id");
            entity.Property(e => e.RoleKey)
                .HasMaxLength(50)
                .HasColumnName("role_key");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("role_permissions_permission_id_fkey"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("role_permissions_role_id_fkey"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("role_permissions_pkey");
                        j.ToTable("role_permissions", "evm");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                        j.IndexerProperty<int>("PermissionId").HasColumnName("permission_id");
                    });
        });

        modelBuilder.Entity<TestDrife>(entity =>
        {
            entity.HasKey(e => e.TestDriveId).HasName("test_drives_pkey");

            entity.ToTable("test_drives", "evm");

            entity.Property(e => e.TestDriveId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("test_drive_id");
            entity.Property(e => e.ConfirmEmail)
                .HasDefaultValue(false)
                .HasColumnName("confirm_email");
            entity.Property(e => e.ConfirmSms)
                .HasDefaultValue(false)
                .HasColumnName("confirm_sms");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.DriveDate).HasColumnName("drive_date");
            entity.Property(e => e.StaffUserId).HasColumnName("staff_user_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.TimeSlot)
                .HasMaxLength(50)
                .HasColumnName("time_slot");
            entity.Property(e => e.VehicleVersionId).HasColumnName("vehicle_version_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.TestDrives)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("test_drives_customer_id_fkey");

            entity.HasOne(d => d.Dealer).WithMany(p => p.TestDrives)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("test_drives_dealer_id_fkey");

            entity.HasOne(d => d.StaffUser).WithMany(p => p.TestDrives)
                .HasForeignKey(d => d.StaffUserId)
                .HasConstraintName("test_drives_staff_user_id_fkey");

            entity.HasOne(d => d.VehicleVersion).WithMany(p => p.TestDrives)
                .HasForeignKey(d => d.VehicleVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("test_drives_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users", "evm");

            entity.HasIndex(e => e.DealerId, "idx_users_dealer");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.LastActivityAt).HasColumnName("last_activity_at");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(200)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");

            entity.HasOne(d => d.Dealer).WithMany(p => p.Users)
                .HasForeignKey(d => d.DealerId)
                .HasConstraintName("users_dealer_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_role_id_fkey");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("vehicles_pkey");

            entity.ToTable("vehicles", "evm");

            entity.HasIndex(e => new { e.Brand, e.ModelName }, "vehicles_brand_model_name_key").IsUnique();

            entity.Property(e => e.VehicleId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("vehicle_id");
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .HasColumnName("brand");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ModelName)
                .HasMaxLength(150)
                .HasColumnName("model_name");
        });

        modelBuilder.Entity<VehicleAllocation>(entity =>
        {
            entity.HasKey(e => e.AllocationId).HasName("vehicle_allocations_pkey");

            entity.ToTable("vehicle_allocations", "evm");

            entity.Property(e => e.AllocationId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("allocation_id");
            entity.Property(e => e.DealerId).HasColumnName("dealer_id");
            entity.Property(e => e.ExpectedDelivery).HasColumnName("expected_delivery");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("request_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.VehicleVersionId).HasColumnName("vehicle_version_id");

            entity.HasOne(d => d.Dealer).WithMany(p => p.VehicleAllocations)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vehicle_allocations_dealer_id_fkey");

            entity.HasOne(d => d.VehicleVersion).WithMany(p => p.VehicleAllocations)
                .HasForeignKey(d => d.VehicleVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vehicle_allocations_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<VehicleTransferOrder>(entity =>
        {
            entity.HasKey(e => e.VehicleTransferOrderId).HasName("vehicle_transfer_order_pkey");

            entity.ToTable("vehicle_transfer_order", "evm");

            entity.Property(e => e.VehicleTransferOrderId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("vehicle_transfer_order_id");
            entity.Property(e => e.FromDealerId).HasColumnName("from_dealer_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.RequestDate).HasColumnName("request_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.ToDealerId).HasColumnName("to_dealer_id");
            entity.Property(e => e.VehicleVersionId).HasColumnName("vehicle_version_id");

            entity.HasOne(d => d.FromDealer).WithMany(p => p.VehicleTransferOrderFromDealers)
                .HasForeignKey(d => d.FromDealerId)
                .HasConstraintName("vehicle_transfer_order_from_dealer_id_fkey");

            entity.HasOne(d => d.ToDealer).WithMany(p => p.VehicleTransferOrderToDealers)
                .HasForeignKey(d => d.ToDealerId)
                .HasConstraintName("vehicle_transfer_order_to_dealer_id_fkey");

            entity.HasOne(d => d.VehicleVersion).WithMany(p => p.VehicleTransferOrders)
                .HasForeignKey(d => d.VehicleVersionId)
                .HasConstraintName("vehicle_transfer_order_vehicle_version_id_fkey");
        });

        modelBuilder.Entity<VehicleVersion>(entity =>
        {
            entity.HasKey(e => e.VehicleVersionId).HasName("vehicle_versions_pkey");

            entity.ToTable("vehicle_versions", "evm");

            entity.HasIndex(e => new { e.VehicleId, e.VersionName, e.Color }, "vehicle_versions_vehicle_id_version_name_color_key").IsUnique();

            entity.Property(e => e.VehicleVersionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("vehicle_version_id");
            entity.Property(e => e.BasePrice)
                .HasPrecision(18, 2)
                .HasColumnName("base_price");
            entity.Property(e => e.Color)
                .HasMaxLength(100)
                .HasColumnName("color");
            entity.Property(e => e.EvType)
                .HasMaxLength(50)
                .HasColumnName("ev_type");
            entity.Property(e => e.HorsePower).HasColumnName("horse_power");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.VersionName)
                .HasMaxLength(150)
                .HasColumnName("version_name");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleVersions)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vehicle_versions_vehicle_id_fkey");
        });

        modelBuilder.Entity<WholesalePrice>(entity =>
        {
            entity.HasKey(e => e.WholesalePriceId).HasName("wholesale_prices_pkey");

            entity.ToTable("wholesale_prices", "evm");

            entity.Property(e => e.WholesalePriceId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("wholesale_price_id");
            entity.Property(e => e.MinOrderQuantity)
                .HasDefaultValue(1)
                .HasColumnName("min_order_quantity");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");
            entity.Property(e => e.VehicleVersionId).HasColumnName("vehicle_version_id");
            entity.Property(e => e.WholesalePrice1)
                .HasPrecision(18, 2)
                .HasColumnName("wholesale_price");

            entity.HasOne(d => d.VehicleVersion).WithMany(p => p.WholesalePrices)
                .HasForeignKey(d => d.VehicleVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("wholesale_prices_vehicle_version_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
