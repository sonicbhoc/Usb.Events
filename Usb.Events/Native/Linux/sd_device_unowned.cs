using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace Usb.Events.Native.Linux;

/// <summary>
/// Represents a device handle that the caller does not own, and therefore does not need to free.
/// </summary>
[SupportedOSPlatform("linux")]
internal class sd_device_unowned() : SafeHandleZeroOrMinusOneIsInvalid(false)
{
    protected override bool ReleaseHandle()
    {
        return true;
    }
}
