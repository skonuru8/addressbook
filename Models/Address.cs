using System.ComponentModel.DataAnnotations;

namespace AddressBookApp.Models;
public class Address
{
    [Key] public int AddressID { get; set; }

    public int CustomerID { get; set; }
    public Customer? Customer { get; set; }

    public int AddressTypeID { get; set; }
    public AddressType? AddressType { get; set; }

    [StringLength(100)] public string? Address1 { get; set; }
    [StringLength(100)] public string? Address2 { get; set; }
    [StringLength(50)]  public string? City { get; set; }
    [StringLength(50)]  public string? State { get; set; }
    [StringLength(50)]  public string? Zip { get; set; }
    [StringLength(50)]  public string? Country { get; set; }
}
