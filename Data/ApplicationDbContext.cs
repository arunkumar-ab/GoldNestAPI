using Microsoft.EntityFrameworkCore;
using GoldNest.Model.Entity;

namespace GoldNest.Data
{
    public class ApplicationDbContext : DbContext
    {
        
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<Loan> Loan { get; set; }
        public DbSet<PawnedItem> PawnedItems { get; set; }
        public DbSet<User> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships and constraints
            //modelBuilder.Entity<Loan>()
            //    .HasOne(l => l.Customer)
            //    .WithMany(c => c.Loans)
            //    .HasForeignKey(l => l.CustomerID)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<PawnedItem>()
            //    .HasOne(pi => pi.Loan)
            //    .WithMany(l => l.PawnedItems)
            //    .HasForeignKey(pi => pi.LoanID)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<PawnedItem>()
            //    .HasOne(pi => pi.Item)
            //    .WithMany(i => i.PawnedItems)
            //    .HasForeignKey(pi => pi.ItemID)
            //    .OnDelete(DeleteBehavior.Restrict);

            //// Unique constraints
            //modelBuilder.Entity<Loan>()
            //    .HasIndex(l => l.BillNo)
            //    .IsUnique();

            //modelBuilder.Entity<User>()
            //    .HasIndex(u => u.Email)
            //    .IsUnique();

            //// Seed initial admin user
            //modelBuilder.Entity<User>().HasData(
            //    new User
            //    {
            //        UserID = 1,
            //        Email = "admin@gmail.com",
            //        PasswordHash = "Admin@123",
            //        Role = "Admin",
            //        CreateDate = DateTime.UtcNow
            //    }
            //);
        }
    }
}