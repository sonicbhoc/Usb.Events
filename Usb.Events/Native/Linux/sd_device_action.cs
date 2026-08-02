using System.Runtime.Versioning;

namespace Usb.Events.Native.Linux;

[SupportedOSPlatform("linux")]
internal enum sd_device_action : long
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
