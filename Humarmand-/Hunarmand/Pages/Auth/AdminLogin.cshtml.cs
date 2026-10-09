using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hunarmand.Helpers;
using Hunarmand.Services;

namespace Hunarmand.Pages.Auth
{
    public class AdminLoginModel : PageModel
    {
        private readonly IUserService _userService;

        public AdminLoginModel(IUserService userService)
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

            var user = await _userService.LoginAsync(Input.Email, Input.Password, "Admin");
            if (user == null)
            {
                ErrorMessage = "Invalid admin credentials or role. Access denied.";
                return Page();
            }

            var adminId = await _userService.GetAdminIdByUserIdAsync(user.UserID);
            if (adminId == null)
            {
                ErrorMessage = "Admin profile mapping failed.";
                return Page();
            }

            // Set session variables
            HttpContext.Session.SetString(SessionKeys.UserId, user.UserID.ToString());
            HttpContext.Session.SetString(SessionKeys.UserRole, user.Role);
            HttpContext.Session.SetString(SessionKeys.UserFullName, user.FullName);
            HttpContext.Session.SetString(SessionKeys.AdminId, adminId.Value.ToString());

            return RedirectToPage("/Admin/Dashboard");
        }
    }
}
