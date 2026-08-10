namespace Usb.Events.Models;

/// <summary>
/// Actions for USB devices.
/// </summary>
public enum UsbDeviceAction
{
    Invalid,
    Plugged,
    Unplugged,
    Mounted,
    Unmounted
}
