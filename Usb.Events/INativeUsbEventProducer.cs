using System.Collections.Generic;
using System.Threading;
using Usb.Events.Models;
using Usb.Events.Native;

namespace Usb.Events;

public interface INativeUsbEventProducer : IUsbEventWatcherLifecycle
{
    IAsyncEnumerable<UsbDeviceEvent> ProduceAsync(CancellationToken cancellationToken = default);
}
