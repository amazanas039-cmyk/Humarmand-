using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hunarmand.Models;
using Hunarmand.Services;

namespace Hunarmand.Pages.Auth
{
    public class CustomerRegisterModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IGeocodingService _geocodingService;

        public CustomerRegisterModel(IUserService userService, IGeocodingService geocodingService)
        {
            _userService = userService;
            _geocodingService = geocodingService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(150)]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = "";

            [Required]
            [EmailAddress]
            [StringLength(150)]
            public string Email { get; set; } = "";

            [Required]
            [Phone]
            [StringLength(20)]
            public string Phone { get; set; } = "";

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = "";

            [StringLength(250)]
            public string Address { get; set; } = "Lahore, Pakistan";
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Resolve geocoding for coordinates
            double lat = -26.2041; // Fallback Johannesburg CBD or similar South Africa/Pakistan
            double lng = 28.0473;

            var coords = await _geocodingService.GeocodeAsync(Input.Address);
            if (coords != null)
            {
                lat = coords.Value.lat;
                lng = coords.Value.lng;
            }

            var customer = new Hunarmand.Models.Customer
            {
                FullName = Input.FullName,
                Email = Input.Email,
                Phone = Input.Phone,
                Address = Input.Address,
                Latitude = (decimal)lat,
                Longitude = (decimal)lng,
                Role = "Customer",
                IsActive = true
            };

            bool success = await _userService.RegisterCustomerAsync(customer, Input.Password);
            if (!success)
            {
                ErrorMessage = "An account with this email address already exists.";
                return Page();
            }

            return RedirectToPage("/Auth/CustomerLogin", new { registered = 1 });
        }
    }
}
