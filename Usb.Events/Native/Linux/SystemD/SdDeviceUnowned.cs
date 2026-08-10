using System.Runtime.Versioning;

namespace Usb.Events.Native.Linux.SystemD;

[SupportedOSPlatform("linux")]
internal partial class SdDeviceUnowned() : SdDevice(false)
{
    private partial class NativeMethods
    {

    }
}
