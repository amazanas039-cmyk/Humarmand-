using System.Collections.Generic;
using Hunarmand.Models;

namespace Hunarmand.Services
{
    public interface IMatchingService
    {
        IEnumerable<Labourer> RankAndSort(IEnumerable<Labourer> labourers, double? customerLat, double? customerLng);
    }
}
