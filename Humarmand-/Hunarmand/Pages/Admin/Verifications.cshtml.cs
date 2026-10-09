using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hunarmand.Models;
using Hunarmand.Services;

namespace Hunarmand.Pages.Admin
{
    public class VerificationsModel : PageModel
    {
        private readonly ILabourerService _labourerService;
        public VerificationsModel(ILabourerService labourerService) { _labourerService = labourerService; }

        public List<Hunarmand.Models.Labourer> Pending { get; set; } = new();
        public string? Message { get; set; }
        public string MessageType { get; set; } = "success";

        public async Task OnGetAsync()
        {
            if (TempData["Msg"] is string msg) { Message = msg; MessageType = TempData["MsgType"] as string ?? "success"; }
            try { Pending = (await _labourerService.GetPendingVerificationAsync()).ToList(); }
            catch { Message = "Failed to load pending verifications."; MessageType = "error"; }
        }

        public async Task<IActionResult> OnPostApproveAsync(int labourerId, string note)
        {
            try
            {
                await _labourerService.ApproveAsync(labourerId, note);
                TempData["Msg"] = "Labourer approved successfully!";
                TempData["MsgType"] = "success";
            }
            catch { TempData["Msg"] = "Approval failed."; TempData["MsgType"] = "error"; }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int labourerId, string note)
        {
            try
            {
                await _labourerService.RejectAsync(labourerId, note);
                TempData["Msg"] = "Labourer rejected.";
                TempData["MsgType"] = "error";
            }
            catch { TempData["Msg"] = "Rejection failed."; TempData["MsgType"] = "error"; }
            return RedirectToPage();
        }
    }
}
