using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddressBookApp.Models;
public class AddressType
{
    [Key] public int AddressTypeID { get; set; }

    // DB column is spelled AdressTypeDescription in your script
    [Required, StringLength(50)]
    [Column("AdressTypeDescription")]
    public string Description { get; set; } = string.Empty;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
