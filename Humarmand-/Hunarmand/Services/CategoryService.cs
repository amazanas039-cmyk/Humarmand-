using System.Collections.Generic;
using Hunarmand.Models;

namespace Hunarmand.Services
{
    public class CategoryService : ICategoryService
    {
        // In a real implementation this would probably query a repository or database.
        // For now we return a static list of categories as a placeholder.
        private static readonly List<SkillCategory> _categories = new List<SkillCategory>
        {
            new SkillCategory { CategoryID = 1, Name = "Plumbing", Description = "Plumbing services", IsActive = true, IconClass = "fa-wrench" },
            new SkillCategory { CategoryID = 2, Name = "Electrical", Description = "Electrical work", IsActive = true, IconClass = "fa-bolt" },
            new SkillCategory { CategoryID = 3, Name = "Carpentry", Description = "Carpentry and woodwork", IsActive = true, IconClass = "fa-hammer" }
        };

        public IEnumerable<SkillCategory> GetAllCategories()
        {
            return _categories;
        }
    }
}
