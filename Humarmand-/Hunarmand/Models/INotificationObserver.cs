using System.Threading.Tasks;

namespace Hunarmand.Models
{
    public interface INotificationObserver
    {
        Task Notify(JobRequest request, string message);
    }
}
