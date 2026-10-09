using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hunarmand.Models;
using Hunarmand.Repositories;

namespace Hunarmand.Pages.Admin
{
    public class LabourersModel : PageModel
    {
        private readonly LabourerRepository _labourerRepo;
        public LabourersModel(LabourerRepository labourerRepo) { _labourerRepo = labourerRepo; }

        public List<Hunarmand.Models.Labourer> Labourers { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Get all labourers via the pending list + approved — reuse SearchNearby with wide radius
                // or use GetPendingVerification for now; in production wire up a GetAll SP.
                var pending = await _labourerRepo.GetPendingVerification();
                Labourers = pending.ToList();
            }
            catch
            {
                ErrorMessage = "Could not load labourers.";
            }
        }
    }
}
