using System.Threading;
using System.Threading.Tasks;
using Usb.Events.Native;

namespace Usb.Events;

public interface IUsbEventWatcherLifecycle
{
    bool IsRunning { get; }
    Task StartAsync(UserData userData, CancellationToken cancellationToken = default);
    Task StopAsync();
}
