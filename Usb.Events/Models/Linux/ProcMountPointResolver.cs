using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Events.Models.Linux;

[SupportedOSPlatform("linux")]
internal sealed class ProcMountPointResolver : IMountPointResolver
{
    private readonly ProcMounts _procMounts = new();

    public async Task<MountPoint> FindMountPointAsync(DeviceNode deviceNode, CancellationToken cancellationToken)
    {
        var mountEntry = await
            _procMounts.ReadAsync()
                .FirstOrDefaultAsync(entry => entry.Device.Value == deviceNode.Value, cancellationToken);

        return mountEntry?.MountPoint ?? MountPoint.NotMounted;
    }
}
