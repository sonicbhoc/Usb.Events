using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace Usb.Events.Native.Linux;

[SupportedOSPlatform("linux")]
internal class sd_device_monitor_unowned() : SafeHandleZeroOrMinusOneIsInvalid(false)
{
    protected override bool ReleaseHandle() => true;
}
