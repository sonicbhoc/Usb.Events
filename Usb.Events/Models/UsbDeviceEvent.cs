using Usb.Events.Native;

namespace Usb.Events.Models;

public record UsbDeviceEvent()
{
    public UsbDeviceEvent(UsbDeviceAction action, UsbDevice device, UserData? context) : this()
    {
        Action = action;
        Device = device;
        Context = context;
    }
    public required UsbDeviceAction Action { get; init; }
    public required UsbDevice Device { get; init; }
    public required UserData? Context { get; init; }
}
