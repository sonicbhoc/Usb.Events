using System;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace Usb.Events.Native.Linux;

[SupportedOSPlatform("linux")]
internal class sd_device_monitor() : SafeHandleZeroOrMinusOneIsInvalid(true)
{
    protected override bool ReleaseHandle()
    {
        return libsystemd.sd_device_monitor_unref(this).handle == IntPtr.Zero;
    }
}
