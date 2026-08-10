using System.Runtime.Versioning;
using Usb.Events.Models;

namespace Usb.Events.Native.Linux.SystemD;

[SupportedOSPlatform("linux")]
internal enum SdDeviceAction : long
{
    SD_DEVICE_ADD,
    SD_DEVICE_REMOVE,
    SD_DEVICE_CHANGE,
    SD_DEVICE_MOVE,
    SD_DEVICE_ONLINE,
    SD_DEVICE_OFFLINE,
    SD_DEVICE_BIND,
    SD_DEVICE_UNBIND,
    SD_DEVICE_ACTION_MAX,
    SD_DEVICE_ACTION_INVALID = -22 // EINVAL from errno.h
}

[SupportedOSPlatform("linux")]
internal static class SdDeviceActionExtensions
{
    internal static UsbDeviceAction ToDeviceAction(this SdDeviceAction sdDeviceAction)
    {

        return sdDeviceAction switch
        {
            SdDeviceAction.SD_DEVICE_ADD or
                SdDeviceAction.SD_DEVICE_BIND or
                SdDeviceAction.SD_DEVICE_ONLINE
                => UsbDeviceAction.Plugged,
            SdDeviceAction.SD_DEVICE_REMOVE or
                SdDeviceAction.SD_DEVICE_UNBIND or
                SdDeviceAction.SD_DEVICE_OFFLINE
                => UsbDeviceAction.Unplugged,
            SdDeviceAction.SD_DEVICE_ACTION_INVALID
                => UsbDeviceAction.Invalid,
            _
                => default
        };
    }
}
