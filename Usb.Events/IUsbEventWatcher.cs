using System;
using Usb.Events.Models;

namespace Usb.Events;

/// <summary>
/// Main Usb.Events interface
/// </summary>
public interface IUsbEventWatcher : IUsbEventWatcherContext, IUsbEventWatcherLifecycle
{
    event EventHandler<UsbDeviceEvent>? OnUsbDeviceMounted;
    event EventHandler<UsbDeviceEvent>? OnUsbDeviceEjected;
    event EventHandler<UsbDeviceEvent>? OnUsbDeviceAdded;
    event EventHandler<UsbDeviceEvent>? OnUsbDeviceRemoved;
}
