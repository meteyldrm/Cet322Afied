using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Cet322Afied.Models
{
    public partial class AfiedDB_322Context : DbContext
    {
        public AfiedDB_322Context()
        {
        }

        public AfiedDB_322Context(DbContextOptions<AfiedDB_322Context> options)
            : base(options)
        {
        }

        public virtual DbSet<TblCustomerUser> TblCustomerUser { get; set; }
        public virtual DbSet<TblManagerUser> TblManagerUser { get; set; }
        public virtual DbSet<TblOrder> TblOrder { get; set; }
        public virtual DbSet<TblProduct> TblProduct { get; set; }
        public virtual DbSet<TblProductCategory> TblProductCategory { get; set; }
        public virtual DbSet<TblProductOrder> TblProductOrder { get; set; }
        public virtual DbSet<TblUser> TblUser { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("data source=MAVERICK;initial catalog=AfiedDB_322;persist security info=True;user id=virtualLogin;password=virtualPassword;MultipleActiveResultSets=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblCustomerUser>(entity =>
            {
                entity.HasKey(e => e.CustomerId)
                    .HasName("PK__TblCusto__B611CB9D8733F783");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("customerID")
                    .ValueGeneratedNever();

                entity.Property(e => e.CustomerAddress)
                    .IsRequired()
                    .HasColumnName("customerAddress")
                    .HasMaxLength(127)
                    .IsUnicode(false);

                entity.HasOne(d => d.Customer)
                    .WithOne(p => p.TblCustomerUser)
                    .HasForeignKey<TblCustomerUser>(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomerUser_ID");
            });

            modelBuilder.Entity<TblManagerUser>(entity =>
            {
                entity.HasKey(e => e.ManagerId)
                    .HasName("PK__TblManag__47E0147F5C074A9E");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("managerID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ManagerAuthorizationLevel).HasColumnName("managerAuthorizationLevel");

                entity.HasOne(d => d.Manager)
                    .WithOne(p => p.TblManagerUser)
                    .HasForeignKey<TblManagerUser>(d => d.ManagerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ManagerUser_ID");
            });

            modelBuilder.Entity<TblOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId)
                    .HasName("PK__TblOrder__0809337D7A178537");

                entity.Property(e => e.OrderId).HasColumnName("orderID");

                entity.Property(e => e.OrderCustomerId).HasColumnName("orderCustomerID");

                entity.Property(e => e.OrderDate)
                    .HasColumnName("orderDate")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.OrderCustomer)
                    .WithMany(p => p.TblOrder)
                    .HasForeignKey(d => d.OrderCustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_CustomerID");
            });

            modelBuilder.Entity<TblProduct>(entity =>
            {
                entity.HasKey(e => e.ProductId)
                    .HasName("PK__TblProdu__2D10D14AB5264759");

                entity.Property(e => e.ProductId).HasColumnName("productID");

                entity.Property(e => e.ProductCategory).HasColumnName("productCategory");

                entity.Property(e => e.ProductMeasurementUnit)
                    .HasColumnName("productMeasurementUnit")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('count')");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasColumnName("productName")
                    .HasMaxLength(63)
                    .IsUnicode(false);

                entity.Property(e => e.ProductPrice)
                    .HasColumnName("productPrice")
                    .HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.ProductCategoryNavigation)
                    .WithMany(p => p.TblProduct)
                    .HasForeignKey(d => d.ProductCategory)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_Category");
            });

            modelBuilder.Entity<TblProductCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId)
                    .HasName("PK__TblProdu__23CAF1F8BA9AC548");

                entity.Property(e => e.CategoryId).HasColumnName("categoryID");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasColumnName("categoryName")
                    .HasMaxLength(63)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblProductOrder>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.ProductId })
                    .HasName("PK_ProductOrder");

                entity.Property(e => e.OrderId).HasColumnName("orderID");

                entity.Property(e => e.ProductId).HasColumnName("productID");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.TblProductOrder)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrder_Order");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.TblProductOrder)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrder_Product");
            });

            modelBuilder.Entity<TblUser>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__TblUser__CB9A1CDFAE647C12");

                entity.Property(e => e.UserId).HasColumnName("userID");

                entity.Property(e => e.UserEmail)
                    .IsRequired()
                    .HasColumnName("userEmail")
                    .HasMaxLength(127)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasColumnName("userName")
                    .HasMaxLength(63)
                    .IsUnicode(true);

                entity.Property(e => e.UserPasswordHash)
                    .IsRequired()
                    .HasColumnName("userPasswordHash")
                    .HasMaxLength(256)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.UserPhone)
                    .IsRequired()
                    .HasColumnName("userPhone")
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
