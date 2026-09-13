using System.Threading;
using System.Threading.Tasks;
using Usb.Events.Models;

namespace Usb.Events;

public interface IMountPointResolver
{
    Task<MountPoint> FindMountPointAsync(DeviceNode device, CancellationToken cancellationToken = default);
}
