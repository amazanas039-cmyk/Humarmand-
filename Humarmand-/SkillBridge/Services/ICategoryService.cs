using System.Collections.Generic;
using SkillBridge.Models;

namespace SkillBridge.Services
{
    public interface ICategoryService
    {
        IEnumerable<SkillCategory> GetAllCategories();
    }
}
