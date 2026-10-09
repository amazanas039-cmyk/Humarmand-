using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hunarmand.Models;
using Hunarmand.Services;

namespace Hunarmand.Pages.Admin
{
    public class CategoriesModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        public CategoriesModel(ICategoryService categoryService) { _categoryService = categoryService; }

        public List<SkillCategory> Categories { get; set; } = new();
        public string? Message { get; set; }
        public string MessageType { get; set; } = "success";

        public void OnGet()
        {
            if (TempData["Msg"] is string msg) { Message = msg; MessageType = TempData["MsgType"] as string ?? "success"; }
            try { Categories = _categoryService.GetAllCategories().ToList(); }
            catch { Message = "Failed to load categories."; MessageType = "error"; }
        }

        public IActionResult OnPostAdd(string name, string iconClass, string? description)
        {
            // In production, call a repository method to insert the category.
            // CategoryService currently reads from config; wire DB insert when SP exists.
            TempData["Msg"] = $"Category '{name}' submitted (DB insert SP needed to persist).";
            TempData["MsgType"] = "success";
            return RedirectToPage();
        }
    }
}
