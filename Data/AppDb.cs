using AddressBookApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AddressBookApp.Data;
public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<AddressType> AddressTypes => Set<AddressType>();
    public DbSet<Address> Addresses => Set<Address>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Customer>().ToTable("Customer");
        b.Entity<AddressType>().ToTable("AddressType");
        b.Entity<Address>().ToTable("Address");

        b.Entity<Address>()
            .HasOne(a => a.Customer).WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CustomerID).OnDelete(DeleteBehavior.Cascade);

        b.Entity<Address>()
            .HasOne(a => a.AddressType).WithMany(t => t.Addresses)
            .HasForeignKey(a => a.AddressTypeID);

        // One address per type per customer
        b.Entity<Address>().HasIndex(a => new { a.CustomerID, a.AddressTypeID }).IsUnique();

        // Unique CustomerNumber
        b.Entity<Customer>().HasIndex(c => c.CustomerNumber).IsUnique();

        // Seed the two types (match your script)
        b.Entity<AddressType>().HasData(
            new AddressType { AddressTypeID = 1, Description = "Home" },
            new AddressType { AddressTypeID = 2, Description = "Work" }
        );
    }
}
