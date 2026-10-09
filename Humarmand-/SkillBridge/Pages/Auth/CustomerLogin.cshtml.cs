using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Helpers;
using SkillBridge.Services;

namespace SkillBridge.Pages.Auth
{
    public class CustomerLoginModel : PageModel
    {
        private readonly IUserService _userService;

        public CustomerLoginModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";
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

            var user = await _userService.LoginAsync(Input.Email, Input.Password, "Customer");
            if (user == null)
            {
                ErrorMessage = "Invalid email, password, or role. Please try again.";
                return Page();
            }

            var customerId = await _userService.GetCustomerIdByUserIdAsync(user.UserID);
            if (customerId == null)
            {
                ErrorMessage = "Customer profile not found for this account.";
                return Page();
            }

            // Set session variables
            HttpContext.Session.SetString(SessionKeys.UserId, user.UserID.ToString());
            HttpContext.Session.SetString(SessionKeys.UserRole, user.Role);
            HttpContext.Session.SetString(SessionKeys.UserFullName, user.FullName);
            HttpContext.Session.SetString(SessionKeys.CustomerId, customerId.Value.ToString());

            return RedirectToPage("/Customer/Dashboard");
        }
    }
}
