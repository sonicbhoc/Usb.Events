using System;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace Usb.Events.Native.Linux;

/// <summary>
/// Represents a device handle that the caller owns and needs to free.
/// </summary>
[SupportedOSPlatform("linux")]
internal class sd_device() : SafeHandleZeroOrMinusOneIsInvalid(true)
{
    protected override bool ReleaseHandle()
    {
        return libsystemd.sd_device_unref(this).handle == IntPtr.Zero;
    }
}
