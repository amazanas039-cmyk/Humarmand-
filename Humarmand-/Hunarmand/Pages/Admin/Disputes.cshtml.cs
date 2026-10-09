using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hunarmand.Models;
using Hunarmand.Services;

namespace Hunarmand.Pages.Admin
{
    public class DisputesModel : PageModel
    {
        private readonly IDisputeService _disputeService;
        public DisputesModel(IDisputeService disputeService) { _disputeService = disputeService; }

        public List<Dispute> OpenDisputes { get; set; } = new();
        public string? Message { get; set; }
        public string MessageType { get; set; } = "success";

        public async Task OnGetAsync()
        {
            if (TempData["Msg"] is string msg) { Message = msg; MessageType = TempData["MsgType"] as string ?? "success"; }
            try { OpenDisputes = (await _disputeService.GetOpenAsync()).ToList(); }
            catch { Message = "Failed to load disputes."; MessageType = "error"; }
        }

        public async Task<IActionResult> OnPostResolveAsync(int disputeId, string resType, string resText)
        {
            // AdminUserID = 1 (hardcoded; wire to session in production)
            try
            {
                await _disputeService.ResolveAsync(disputeId, resType, resText, 1);
                TempData["Msg"] = $"Dispute #{disputeId} resolved as '{resType}'.";
                TempData["MsgType"] = "success";
            }
            catch { TempData["Msg"] = "Resolution failed."; TempData["MsgType"] = "error"; }
            return RedirectToPage();
        }
    }
}
