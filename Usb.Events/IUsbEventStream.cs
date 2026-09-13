using System.Collections.Generic;
using System.Threading;
using Usb.Events.Models;

namespace Usb.Events;

public interface IUsbEventStream : IUsbEventWatcherContext, IUsbEventWatcherLifecycle
{
    IAsyncEnumerable<UsbDeviceEvent> MonitorAsync( CancellationToken cancellationToken = default);
}
