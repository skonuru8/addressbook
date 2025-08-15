using System.ComponentModel.DataAnnotations;

namespace AddressBookApp.Models;
public class Customer
{
    [Key] public int CustomerID { get; set; }

    // “Digits only” field to satisfy the assignment
    [Required, Range(0, int.MaxValue, ErrorMessage = "Digits only")]
    public int CustomerNumber { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
