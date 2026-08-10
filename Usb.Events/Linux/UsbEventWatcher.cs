using System;

namespace Usb.Events.Linux;

public class UsbEventWatcher
{
    public event EventHandler<UsbDeviceEventArgs>? DeviceInserted;
    public event EventHandler<UsbDeviceEventArgs>? DeviceRemoved;
}

public class UsbDeviceEventArgs : EventArgs
{
}
