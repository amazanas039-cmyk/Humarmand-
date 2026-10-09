using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SkillBridge.Models;

namespace SkillBridge.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<SkillCategory> Categories { get; set; } = new();
        public List<SkillBridge.Models.Labourer> FeaturedLabourers { get; set; } = new();

        public void OnGet()
        {
            // Seed Mock Categories for Phase 1 (will load from Db in Phase 2)
            Categories = new List<SkillCategory>
            {
                new() { CategoryID = 1, Name = "Electrician", Description = "Wiring, installations, repairs", IsActive = true, IconClass = "fa-solid fa-bolt" },
                new() { CategoryID = 2, Name = "Plumber", Description = "Pipes, drains, fixtures", IsActive = true, IconClass = "fa-solid fa-faucet" },
                new() { CategoryID = 3, Name = "Carpenter", Description = "Furniture, doors, cabinetry", IsActive = true, IconClass = "fa-solid fa-hammer" },
                new() { CategoryID = 4, Name = "Painter", Description = "Interior/exterior painting", IsActive = true, IconClass = "fa-solid fa-paint-roller" },
                new() { CategoryID = 5, Name = "Mason", Description = "Brickwork, plastering, tiles", IsActive = true, IconClass = "fa-solid fa-trowel" },
                new() { CategoryID = 6, Name = "AC Technician", Description = "AC, heating, ventilation", IsActive = true, IconClass = "fa-solid fa-wind" },
                new() { CategoryID = 7, Name = "Welder", Description = "Metal fabrication and welding", IsActive = true, IconClass = "fa-solid fa-compact-disc" },
                new() { CategoryID = 8, Name = "Solar Panel Technician", Description = "Solar installation and maintenance", IsActive = true, IconClass = "fa-solid fa-solar-panel" }
            };

            // Seed Mock Labourers for Phase 1 (will load from Db in Phase 2)
            FeaturedLabourers = new List<SkillBridge.Models.Labourer>
            {
                new() 
                { 
                    LabourerID = 1, 
                    FullName = "Muhammad Ali", 
                    CategoryName = "Electrician", 
                    ReputationScore = 4.80m, 
                    CompletionRate = 95.00m, 
                    HourlyRate = 450.00m, 
                    ExperienceYears = 8, 
                    Bio = "Experienced commercial and residential electrician. Specializing in fault diagnostic and lighting installation.",
                    IsProfileLive = true 
                },
                new() 
                { 
                    LabourerID = 2, 
                    FullName = "Sajid Mahmood", 
                    CategoryName = "Plumber", 
                    ReputationScore = 4.50m, 
                    CompletionRate = 90.00m, 
                    HourlyRate = 350.00m, 
                    ExperienceYears = 5, 
                    Bio = "Professional plumbing services. Expert in leak repairs, geyser service, and pipeline installations.",
                    IsProfileLive = true 
                },
                new() 
                { 
                    LabourerID = 3, 
                    FullName = "Zafar Iqbal", 
                    CategoryName = "Painter", 
                    ReputationScore = 4.20m, 
                    CompletionRate = 85.00m, 
                    HourlyRate = 300.00m, 
                    ExperienceYears = 3, 
                    Bio = "Expert interior and exterior painting services. Wall putty, color mixing, and wallpaper installation.",
                    IsProfileLive = true 
                },
                new() 
                { 
                    LabourerID = 4, 
                    FullName = "Javed Malik", 
                    CategoryName = "Carpenter", 
                    ReputationScore = 4.90m, 
                    CompletionRate = 98.00m, 
                    HourlyRate = 500.00m, 
                    ExperienceYears = 10, 
                    Bio = "Master carpenter specialized in wooden wardrobes, kitchen fittings, door repairs, and modular furniture.",
                    IsProfileLive = true 
                }
            };
        }
    }
}
