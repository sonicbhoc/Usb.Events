using System;
using System.Collections.Generic;
using System.Threading;
using Usb.Events.Models;
using Usb.Events.Native;

namespace Usb.Events;

public interface IUsbEventStream : IDisposable
{
    IAsyncEnumerable<UsbDeviceEvent> MonitorAsync(
        StartingBehavior startingBehavior,
        UserData userData,
        CancellationToken cancellationToken = default);
}
