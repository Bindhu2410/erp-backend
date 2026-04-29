using Microsoft.EntityFrameworkCore;
using ERP.API.Models;

namespace ERP.API
{
    public class AppDbContext : DbContext
    {
        public DbSet<Receipt> Receipts { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SalesLead> SalesLeads { get; set; }
        public DbSet<SalesDemo> SalesDemo { get; set; }
        public DbSet<SalesCustomer> SalesCustomers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SalesEmployee> SalesEmployees { get; set; }
        public DbSet<EmployeeAllowance> EmployeeAllowances { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<TeamHierarchy> TeamHierarchy { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<BomName> BomNames { get; set; }
        public DbSet<InventoryMethod> InventoryMethods { get; set; }
        public DbSet<InventoryType> InventoryTypes { get; set; }
        public DbSet<InventoryGroup> InventoryGroups { get; set; }
        public DbSet<ValuationMethod> ValuationMethods { get; set; }
        public DbSet<Uom> Uoms { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Make> Makes { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ItemMaster> ItemMasters { get; set; }
        public DbSet<BillOfMaterial> BillOfMaterials { get; set; }
        public DbSet<BillOfMaterialOptionalItem> BillOfMaterialOptionalItems { get; set; }
        public DbSet<BomItemMapping> BomItemMappings { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<ItemLocation> ItemLocations { get; set; }
        public DbSet<ItemStock> ItemStocks { get; set; }
        public DbSet<ItemLocationStock> ItemLocationStocks { get; set; }
        public DbSet<ItemPlanning> ItemPlannings { get; set; }
        public DbSet<ItemUomPackingDetails> ItemUomPackingDetails { get; set; }
        public DbSet<ItemAccountingInfo> ItemAccountingInfos { get; set; }
        public DbSet<ItemQualityControl> ItemQualityControls { get; set; }
        public DbSet<ItemStockTransaction> ItemStockTransactions { get; set; }
        // Add other DbSets as needed
        public DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; set; }
        public DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems { get; set; }
        public DbSet<Models.RateMaster> RateMasters { get; set; }
        public DbSet<RateMasterItem> RateMasterItems { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<IssueOptionalItem> IssueOptionalItems { get; set; }
        public DbSet<IssueItem> IssueItems { get; set; }
        public DbSet<ReceiptItem> ReceiptItems { get; set; }
        public DbSet<ReceiptOptionalItem> ReceiptOptionalItems { get; set; }
        public DbSet<ReceiptAccessory> ReceiptAccessories { get; set; }
    public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
    public DbSet<PurchaseRequisitionBom> PurchaseRequisitionBoms { get; set; }
        public DbSet<QcTemplate> QcTemplates { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimItem> ClaimItems { get; set; }
        public DbSet<ClaimVoucherItem> ClaimVoucherItems { get; set; }
        public DbSet<ClaimVoucher> ClaimVouchers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Models.Chat.Chat> Chats { get; set; }
        public DbSet<Models.Chat.ChatMember> ChatMembers { get; set; }
        public DbSet<Models.Chat.ChatMessageV2> ChatMessagesV2 { get; set; }
        public DbSet<Models.Chat.MessageStatus> MessageStatuses { get; set; }
        public DbSet<Models.Chat.UserPresence> UserPresences { get; set; }
        public DbSet<QuotationTitle> QuotationTitles { get; set; }
        public DbSet<SalesTermsAndConditions> SalesTermsAndConditions { get; set; }

        public DbSet<AccessoriesHeader> AccessoriesHeaders { get; set; }
        public DbSet<AccessoriesDetails> AccessoriesDetails { get; set; }
        // WhatsApp
        public DbSet<WhatsAppAccount> WhatsAppAccounts { get; set; }
        public DbSet<WhatsAppConversation> WhatsAppConversations { get; set; }
        public DbSet<WhatsAppMessage> WhatsAppMessages { get; set; }
        public DbSet<WhatsAppTemplate> WhatsAppTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Issue>().ToTable("issues");
            modelBuilder.Entity<IssueOptionalItem>().ToTable("issue_optional_items");
            modelBuilder.Entity<IssueItem>().ToTable("issue_items");
            modelBuilder.Entity<Receipt>().ToTable("receipt");
            modelBuilder.Entity<ReceiptItem>().ToTable("receipt_items");
            modelBuilder.Entity<ReceiptOptionalItem>().ToTable("receipt_optional_items");
            modelBuilder.Entity<ReceiptAccessory>().ToTable("receipt_accessories");

            // Configure ItemMaster to ensure all fields are mapped correctly
            modelBuilder.Entity<ItemMaster>(entity =>
            {
                entity.ToTable("item_master");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemDescription).HasColumnName("item_description");
                entity.Property(e => e.LongItemName).HasColumnName("long_item_name");
                entity.Property(e => e.Criticality).HasColumnName("criticality");
                entity.Property(e => e.StockToBank).HasColumnName("stock_to_bank");
                entity.Property(e => e.LpRate).HasColumnName("lp_rate");
                entity.Property(e => e.Specification).HasColumnName("specification");
                entity.Property(e => e.ValuationMethodText).HasColumnName("valuation_method");
                entity.Property(e => e.RelatedStockAccount).HasColumnName("related_stock_account");
                entity.Property(e => e.Cf).HasColumnName("cf");
                entity.Property(e => e.BomApplicable).HasColumnName("bom_applicable");
                entity.Property(e => e.InventoryType).HasColumnName("inventory_type");
                entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            });

            // Configure Department
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("departments");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Id).HasColumnName("id");
                entity.Property(d => d.Name).HasColumnName("name");
                entity.Property(d => d.HeadOfDepartment).HasColumnName("head_of_department");
                entity.Property(d => d.UserCreated).HasColumnName("user_created");
                entity.Property(d => d.DateCreated).HasColumnName("date_created");
                entity.Property(d => d.UserUpdated).HasColumnName("user_updated");
                entity.Property(d => d.DateUpdated).HasColumnName("date_updated");
            });

            // Configure Designation
            modelBuilder.Entity<Designation>(entity =>
            {
                entity.ToTable("designation");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Id).HasColumnName("id");
                entity.Property(d => d.Code).HasColumnName("code");
                entity.Property(d => d.Name).HasColumnName("name");
                entity.Property(d => d.UserCreated).HasColumnName("user_created");
                entity.Property(d => d.DateCreated).HasColumnName("date_created");
                entity.Property(d => d.UserUpdated).HasColumnName("user_updated");
                entity.Property(d => d.DateUpdated).HasColumnName("date_updated");
            });

            // Configure SalesEmployee
            modelBuilder.Entity<SalesEmployee>(entity =>
            {
                entity.ToTable("employees");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.EmployeeId)
                    .HasDefaultValueSql("('EMP-' || LPAD(nextval('employee_id_seq')::text, 3, '0'))");
                
                entity.HasMany(e => e.EmployeeAllowances)
                    .WithOne(a => a.SalesEmployee)
                    .HasForeignKey(a => a.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                    // Link to User (optional 1:1)
                    entity.Property(e => e.UserId).HasColumnName("user_id");
                    entity.HasIndex(e => e.UserId)
                        .IsUnique()
                        .HasName("ux_employees_user_id");
                    entity.HasOne(e => e.User)
                        .WithOne()
                        .HasForeignKey<SalesEmployee>(e => e.UserId)
                        .OnDelete(DeleteBehavior.SetNull);
                
                entity.Property(e => e.Active).HasColumnName("active");
            });

            // Configure EmployeeAllowance
            modelBuilder.Entity<EmployeeAllowance>(entity =>
            {
                entity.ToTable("employee_allowances");
                entity.HasKey(a => a.Id);
                
                entity.HasIndex(a => a.EmployeeId)
                    .HasName("idx_employee_allowance_employee_id");

                entity.Property(e => e.Active).HasColumnName("active");
            });

            // Configure Attendance
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.ToTable("attendance");
                entity.HasKey(a => a.Id);
                
                entity.HasOne(a => a.SalesEmployee)
                    .WithMany()
                    .HasForeignKey(a => a.EmployeeId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(a => new { a.EmployeeId, a.AttendanceDate })
                    .IsUnique()
                    .HasName("idx_attendance_employee_date");
                
                entity.HasIndex(a => a.EmployeeId)
                    .HasName("idx_attendance_employee_id");
                
                entity.HasIndex(a => a.AttendanceDate)
                    .HasName("idx_attendance_date");
            });

            // For PurchaseRequisition
            modelBuilder.Entity<PurchaseRequisition>(entity =>
            {
                // Removed BomIds property mapping
            });

            // For PurchaseRequisitionBom
            modelBuilder.Entity<PurchaseRequisitionBom>(entity =>
            {
                entity.ToTable("purchase_requisition_boms");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PurchaseRequisitionId).HasColumnName("purchase_requisition_id");
                entity.Property(e => e.ItemId).HasColumnName("item_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
            });

            // Configure Claim and ClaimItem relationship
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.ToTable("claims");
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.Items)
                    .WithOne(i => i.Claim)
                    .HasForeignKey(i => i.ClaimId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ClaimVoucher (claim_voucher table)
            modelBuilder.Entity<Models.ClaimVoucher>(entity =>
            {
                entity.ToTable("claim_voucher");
                entity.HasKey(e => e.Id);
                // Column mappings are done via attributes on the model
                    // Configure relationship to claim_voucher_items
                    entity.HasMany(e => e.Items)
                        .WithOne(i => i.ClaimVoucher)
                        .HasForeignKey(i => i.ClaimVoucherId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClaimItem>(entity =>
            {
                entity.ToTable("claim_items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClaimId).HasColumnName("claim_id");
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("chat_messages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.User).HasColumnName("user");
                entity.Property(e => e.Message).HasColumnName("message");
                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                entity.Property(e => e.GroupName).HasColumnName("group_name");
            });

            // Chat V2 entities
            modelBuilder.Entity<Models.Chat.Chat>(entity =>
            {
                entity.ToTable("chats");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Models.Chat.ChatMember>(entity =>
            {
                entity.ToTable("chat_members");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ChatId, e.UserId }).IsUnique();
            });

            modelBuilder.Entity<Models.Chat.ChatMessageV2>(entity =>
            {
                entity.ToTable("chat_messages_v2");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Models.Chat.MessageStatus>(entity =>
            {
                entity.ToTable("message_status");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.MessageId, e.UserId }).IsUnique();
            });

            modelBuilder.Entity<Models.Chat.UserPresence>(entity =>
            {
                entity.ToTable("user_presence");
                entity.HasKey(e => e.UserId);
            });

            // Configure RateMaster and RateMasterItem relationship
            modelBuilder.Entity<RateMaster>(entity =>
            {
                entity.ToTable("rate_master");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<RateMasterItem>(entity =>
            {
                entity.ToTable("rate_master_items");
                entity.HasKey(e => e.Id);
                entity.HasOne<RateMaster>()
                    .WithMany()
                    .HasForeignKey(e => e.RateMasterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── WhatsApp ──────────────────────────────────────────────────────

            modelBuilder.Entity<WhatsAppAccount>(entity =>
            {
                entity.ToTable("whatsapp_accounts");
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.Conversations)
                    .WithOne(c => c.Account)
                    .HasForeignKey(c => c.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Templates)
                    .WithOne(t => t.Account)
                    .HasForeignKey(t => t.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WhatsAppConversation>(entity =>
            {
                entity.ToTable("whatsapp_conversations");
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.Messages)
                    .WithOne(m => m.Conversation)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WhatsAppMessage>(entity =>
            {
                entity.ToTable("whatsapp_messages");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<WhatsAppTemplate>(entity =>
            {
                entity.ToTable("whatsapp_templates");
                entity.HasKey(e => e.Id);
            });
        }
    }
}
