using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AddressBookApp.Data;   // <-- your AppDb namespace
using AddressBookApp.Models; // <-- your entity namespace (Customer, Address, AddressType)

namespace AddressBookApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDb _db;

        public IndexModel(AppDb db) => _db = db;

        // Data for the page
        public List<Customer> Customers { get; set; } = new();
        public List<Address> Addresses { get; set; } = new();
        public List<AddressType> AddressTypes { get; set; } = new();

        // State (bound from querystring/form)
        [BindProperty(SupportsGet = true)]
        public int? CustomerId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EditCustomerId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EditAddressId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "CustomerNumber";

        public string? Error { get; set; }

        // GET
        public async Task<IActionResult> OnGetAsync(string? sort, int? customerId, int? editCustomerId, int? editAddressId)
        {
            if (!string.IsNullOrWhiteSpace(sort)) Sort = sort;
            if (customerId.HasValue) CustomerId = customerId;
            if (editCustomerId.HasValue) EditCustomerId = editCustomerId;
            if (editAddressId.HasValue) EditAddressId = editAddressId;

            await LoadDataAsync();
            return Page();
        }

        // ---------- Customer handlers ----------
        public async Task<IActionResult> OnPostAddCustomerAsync(int CustomerNumber, string FirstName, string LastName, string sort)
        {
            Sort = sort;

            FirstName = (FirstName ?? "").Trim();
            LastName  = (LastName ?? "").Trim();

            if (CustomerNumber < 0 || string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                Error = "Please provide a non-negative Customer Number and non-empty First/Last names.";
                await LoadDataAsync();
                return Page();
            }

            var c = new Customer { CustomerNumber = CustomerNumber, FirstName = FirstName, LastName = LastName };
            _db.Customers.Add(c);
            await _db.SaveChangesAsync();

            // Select the newly added customer
            return RedirectToPage(new { sort = Sort, customerId = c.CustomerID });
        }

        public async Task<IActionResult> OnPostUpdateCustomerAsync(int Id, int CustomerNumber, string FirstName, string LastName, string sort, int? customerId)
        {
            Sort = sort;

            var c = await _db.Customers.FindAsync(Id);
            if (c == null)
            {
                Error = "Customer not found.";
                await LoadDataAsync();
                return Page();
            }

            if (CustomerNumber < 0 || string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                Error = "Please provide a non-negative Customer Number and non-empty First/Last names.";
                await LoadDataAsync();
                return Page();
            }

            c.CustomerNumber = CustomerNumber;
            c.FirstName = FirstName.Trim();
            c.LastName  = LastName.Trim();
            await _db.SaveChangesAsync();

            return RedirectToPage(new { sort = Sort, customerId = Id });
        }

        public async Task<IActionResult> OnPostDeleteCustomerAsync(int Id, string sort)
        {
            Sort = sort;

            var c = await _db.Customers.FindAsync(Id);
            if (c != null)
            {
                // ensure addresses are removed if cascade isn't configured
                var addrs = _db.Addresses.Where(a => a.CustomerID == Id);
                _db.Addresses.RemoveRange(addrs);

                _db.Customers.Remove(c);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage(new { sort = Sort });
        }

        // ---------- Address handlers ----------
        public async Task<IActionResult> OnPostAddAddressAsync(
            int CustomerID, int AddressTypeID,
            string? Address1, string? Address2, string? City,
            string State, string Zip, string? Country, string sort)
        {
            Sort = sort;

            // Only one address per type for each customer
            bool exists = await _db.Addresses.AnyAsync(a => a.CustomerID == CustomerID && a.AddressTypeID == AddressTypeID);
            if (exists)
            {
                Error = "Each customer can have only one address per address type.";
                await LoadDataAsync(CustomerID);
                return Page();
            }

            if (!IsStateValid(State) || !IsZipValid(Zip))
            {
                Error = "Invalid State or ZIP. State must be 2 letters (e.g., CA). ZIP must be 12345 or 12345-6789.";
                await LoadDataAsync(CustomerID);
                return Page();
            }

            var addr = new Address
            {
                CustomerID = CustomerID,
                AddressTypeID = AddressTypeID,
                Address1 = NullIfBlank(Address1),
                Address2 = NullIfBlank(Address2),
                City     = NullIfBlank(City),
                State    = State.Trim(),
                Zip      = Zip.Trim(),
                Country  = NullIfBlank(Country)
            };
            _db.Addresses.Add(addr);
            await _db.SaveChangesAsync();

            return RedirectToPage(new { sort = Sort, customerId = CustomerID });
        }

        public async Task<IActionResult> OnPostUpdateAddressAsync(
            int Id, int AddressTypeID,
            string? Address1, string? Address2, string? City,
            string State, string Zip, string? Country,
            string sort, int? customerId)
        {
            Sort = sort;

            var addr = await _db.Addresses.FindAsync(Id);
            if (addr == null)
            {
                Error = "Address not found.";
                await LoadDataAsync(customerId);
                return Page();
            }

            // Enforce one address per type (allow keeping same type)
            bool existsOther = await _db.Addresses.AnyAsync(a =>
                a.CustomerID == addr.CustomerID && a.AddressTypeID == AddressTypeID && a.AddressID != Id);
            if (existsOther)
            {
                Error = "Each customer can have only one address per address type.";
                await LoadDataAsync(addr.CustomerID);
                return Page();
            }

            if (!IsStateValid(State) || !IsZipValid(Zip))
            {
                Error = "Invalid State or ZIP. State must be 2 letters (e.g., CA). ZIP must be 12345 or 12345-6789.";
                await LoadDataAsync(addr.CustomerID);
                return Page();
            }

            addr.AddressTypeID = AddressTypeID;
            addr.Address1 = NullIfBlank(Address1);
            addr.Address2 = NullIfBlank(Address2);
            addr.City     = NullIfBlank(City);
            addr.State    = State.Trim();
            addr.Zip      = Zip.Trim();
            addr.Country  = NullIfBlank(Country);
            await _db.SaveChangesAsync();

            return RedirectToPage(new { sort = Sort, customerId = addr.CustomerID });
        }

        public async Task<IActionResult> OnPostDeleteAddressAsync(int Id, string sort, int? customerId)
        {
            Sort = sort;

            var addr = await _db.Addresses.FindAsync(Id);
            if (addr != null)
            {
                int keepCustomerId = addr.CustomerID;
                _db.Addresses.Remove(addr);
                await _db.SaveChangesAsync();
                return RedirectToPage(new { sort = Sort, customerId = keepCustomerId });
            }

            return RedirectToPage(new { sort = Sort, customerId });
        }

        // ---------- Utilities ----------
        private async Task LoadDataAsync(int? forCustomerId = null)
        {
            // Customers (sorted)
            IQueryable<Customer> q = _db.Customers;
            q = Sort switch
            {
                "FirstName" => q.OrderBy(c => c.FirstName).ThenBy(c => c.LastName),
                "LastName"  => q.OrderBy(c => c.LastName).ThenBy(c => c.FirstName),
                _           => q.OrderBy(c => c.CustomerNumber)
            };
            Customers = await q.AsNoTracking().ToListAsync();

            // address types
            AddressTypes = await _db.AddressTypes.AsNoTracking().OrderBy(t => t.AddressTypeID).ToListAsync();

            // addresses for selected customer
            int? targetId = forCustomerId ?? CustomerId;
            if (targetId.HasValue)
            {
                CustomerId = targetId; // keep it
                Addresses = await _db.Addresses
                    .Include(a => a.AddressType)
                    .Where(a => a.CustomerID == targetId.Value)
                    .OrderBy(a => a.AddressTypeID)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else
            {
                Addresses.Clear();
            }
        }

        private static bool IsStateValid(string? state) =>
            !string.IsNullOrWhiteSpace(state) && Regex.IsMatch(state.Trim(), "^[A-Za-z]{2}$");

        private static bool IsZipValid(string? zip) =>
            !string.IsNullOrWhiteSpace(zip) && Regex.IsMatch(zip.Trim(), @"^\d{5}(-\d{4})?$");

        private static string? NullIfBlank(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return s.Trim();
        }
    }
}
