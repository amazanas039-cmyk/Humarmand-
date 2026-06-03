using System.Threading.Tasks;

namespace SkillBridge.Models
{
    public interface INotificationObserver
    {
        Task Notify(JobRequest request, string message);
    }
}
