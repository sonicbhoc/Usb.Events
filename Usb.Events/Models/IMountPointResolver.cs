using System.Threading;
using System.Threading.Tasks;

namespace Usb.Events.Models;

internal interface IMountPointResolver
{
    Task<MountPoint> GetMountPointAsync(DeviceNode deviceNode, CancellationToken cancellationToken);
}
