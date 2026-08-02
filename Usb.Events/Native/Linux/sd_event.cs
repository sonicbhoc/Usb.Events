using System;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace Usb.Events.Native.Linux;

[SupportedOSPlatform("linux")]
internal class sd_event() : SafeHandleZeroOrMinusOneIsInvalid(true)
{
    protected override bool ReleaseHandle()
    {
        throw new NotImplementedException();
    }
}
