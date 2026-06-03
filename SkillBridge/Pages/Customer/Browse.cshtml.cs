using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Helpers;
using SkillBridge.Models;
using SkillBridge.Services;

namespace SkillBridge.Pages.Customer
{
    [RequireRole("Customer")]
    public class BrowseModel : PageModel
    {
        private readonly IMatchingService _matchingService;
        private readonly ILabourerService _labourerService;
        private readonly ICategoryService _categoryService;

        public BrowseModel(IMatchingService matchingService, ILabourerService labourerService, ICategoryService categoryService)
        {
            _matchingService = matchingService;
            _labourerService = labourerService;
            _categoryService = categoryService;
        }

        public IEnumerable<SkillCategory> Categories { get; set; } = new List<SkillCategory>();

        public void OnGet()
        {
            Categories = _categoryService.GetAllCategories();
        }

        public async Task<IActionResult> OnGetResultsAsync(double lat, double lng, double radiusKm, int? categoryId, decimal? minRating, int? minExp)
        {
            var labourers = await _labourerService.SearchNearbyAsync(lat, lng, radiusKm, categoryId, minRating, minExp);
            var ranked = _matchingService.RankAndSort(labourers, lat, lng);
            
            return Partial("_LabourerResults", ranked);
        }
    }
}
