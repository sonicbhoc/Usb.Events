using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Usb.Events.Models;
using Usb.Events.Native;

namespace Usb.Events;

public interface IUsbEventWatcherContext
{
    Task<IReadOnlyCollection<UsbDevice>> GetUsbDevicesAsync(CancellationToken  cancellationToken = default);
    Task<IReadOnlyDictionary<UsbDevice, MountPoint>> GetMountPointsAsync(CancellationToken cancellationToken = default);
}
