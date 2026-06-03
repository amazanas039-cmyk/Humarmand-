using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Models;
using SkillBridge.Services;

namespace SkillBridge.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly IAdminService _adminService;
        public UsersModel(IAdminService adminService) { _adminService = adminService; }

        public List<User> Users { get; set; } = new();
        public string? Search { get; set; }
        public string? Message { get; set; }
        public string MessageType { get; set; } = "success";

        public async Task OnGetAsync(string? search)
        {
            Search = search;
            try
            {
                var result = await _adminService.GetAllUsersAsync(search);
                Users = result.ToList();
            }
            catch
            {
                Message = "Could not load users. Check database connection.";
                MessageType = "error";
            }
        }

        public async Task<IActionResult> OnPostToggleSuspendAsync(int userId, bool suspend)
        {
            try
            {
                await _adminService.SuspendUserAsync(userId, suspend);
                TempData["Msg"] = suspend ? "User suspended successfully." : "User unsuspended successfully.";
                TempData["MsgType"] = "success";
            }
            catch
            {
                TempData["Msg"] = "Action failed. Please try again.";
                TempData["MsgType"] = "error";
            }
            return RedirectToPage();
        }

        public override void OnPageHandlerExecuted(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutedContext context)
        {
            if (TempData["Msg"] is string msg)
            {
                Message = msg;
                MessageType = TempData["MsgType"] as string ?? "success";
            }
            base.OnPageHandlerExecuted(context);
        }
    }
}
