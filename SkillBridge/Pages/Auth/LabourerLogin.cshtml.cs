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
    public class LabourerLoginModel : PageModel
    {
        private readonly IUserService _userService;

        public LabourerLoginModel(IUserService userService)
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

            var user = await _userService.LoginAsync(Input.Email, Input.Password, "Labourer");
            if (user == null)
            {
                ErrorMessage = "Invalid email, password, or role. Please try again.";
                return Page();
            }

            var labourerId = await _userService.GetLabourerIdByUserIdAsync(user.UserID);
            if (labourerId == null)
            {
                ErrorMessage = "Labourer profile not found for this account.";
                return Page();
            }

            // Set session variables
            HttpContext.Session.SetString(SessionKeys.UserId, user.UserID.ToString());
            HttpContext.Session.SetString(SessionKeys.UserRole, user.Role);
            HttpContext.Session.SetString(SessionKeys.UserFullName, user.FullName);
            HttpContext.Session.SetString(SessionKeys.LabourerId, labourerId.Value.ToString());

            return RedirectToPage("/Labourer/Dashboard");
        }
    }
}
