using System.Collections.Generic;
using Hunarmand.Models;

namespace Hunarmand.Services
{
    public interface ICategoryService
    {
        IEnumerable<SkillCategory> GetAllCategories();
    }
}
