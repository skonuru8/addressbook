using AddressBookApp.Data;
using AddressBookApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AddressBookApp.Pages;
public class IndexModel : PageModel
{
    private readonly AppDb _db;
    public IndexModel(AppDb db) => _db = db;

    public List<Customer> Customers { get; set; } = new();
    public List<Address> Addresses { get; set; } = new();
    public List<AddressType> AddressTypes { get; set; } = new();

    [BindProperty(SupportsGet = true)] public int? CustomerId { get; set; }
    [BindProperty(SupportsGet = true)] public string Sort { get; set; } = "CustomerNumber";
    [TempData] public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            IQueryable<Customer> q = _db.Customers;
            q = Sort switch
            {
                "FirstName" => q.OrderBy(c => c.FirstName),
                "LastName"  => q.OrderBy(c => c.LastName),
                _           => q.OrderBy(c => c.CustomerNumber)
            };
            Customers = await q.AsNoTracking().ToListAsync();

            if (CustomerId.HasValue)
            {
                Addresses = await _db.Addresses
                    .Where(a => a.CustomerID == CustomerId.Value)
                    .Include(a => a.AddressType)
                    .OrderBy(a => a.AddressType!.Description)
                    .AsNoTracking().ToListAsync();

                AddressTypes = await _db.AddressTypes.OrderBy(t => t.Description).AsNoTracking().ToListAsync();
            }
        }
        catch (Exception ex) { Error = "Error loading data: " + ex.Message; }
    }

    // Customers
    public async Task<IActionResult> OnPostAddCustomerAsync(int CustomerNumber, string FirstName, string LastName, string sort)
    {
        try
        {
            _db.Customers.Add(new Customer { CustomerNumber = CustomerNumber, FirstName = FirstName, LastName = LastName });
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException) { Error = "That Customer Number already exists."; }
        catch (Exception ex) { Error = "Add customer failed: " + ex.Message; }
        return RedirectToPage(new { sort });
    }

    public async Task<IActionResult> OnPostDeleteCustomerAsync(int Id, string sort)
    {
        try { var c = await _db.Customers.FindAsync(Id); if (c != null) { _db.Customers.Remove(c); await _db.SaveChangesAsync(); } }
        catch (Exception ex) { Error = "Delete failed: " + ex.Message; }
        return RedirectToPage(new { sort });
    }

    // Addresses
    public async Task<IActionResult> OnPostAddAddressAsync(Address input, string sort, int customerId)
    {
        try { _db.Addresses.Add(input); await _db.SaveChangesAsync(); }
        catch (DbUpdateException) { Error = "Each customer can have only one address per address type."; }
        catch (Exception ex) { Error = "Add address failed: " + ex.Message; }
        return RedirectToPage(new { sort, customerId = input.CustomerID });
    }

    public async Task<IActionResult> OnPostDeleteAddressAsync(int Id, string sort, int customerId)
    {
        try { var a = await _db.Addresses.FindAsync(Id); if (a != null) { _db.Addresses.Remove(a); await _db.SaveChangesAsync(); } }
        catch (Exception ex) { Error = "Delete address failed: " + ex.Message; }
        return RedirectToPage(new { sort, customerId });
    }
}
