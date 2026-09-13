using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Usb.Events.Models;

namespace Usb.Events;

public interface INativeUsbDeviceScanner : IUsbEventWatcherLifecycle
{
    ValueTask<IEnumerable<UsbDeviceEvent>> ScanAsync(CancellationToken ct);
}
