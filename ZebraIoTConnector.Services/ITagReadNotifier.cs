using System.Threading.Tasks;

namespace ZebraIoTConnector.Services
{
    public interface ITagReadNotifier
    {
        Task NotifyTagReadAsync(object tagReadData);
    }
}
