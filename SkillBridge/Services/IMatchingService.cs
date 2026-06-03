using System.Collections.Generic;
using SkillBridge.Models;

namespace SkillBridge.Services
{
    public interface IMatchingService
    {
        IEnumerable<Labourer> RankAndSort(IEnumerable<Labourer> labourers, double? customerLat, double? customerLng);
    }
}
