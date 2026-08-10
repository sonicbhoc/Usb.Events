using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Usb.Events.Models;
using Usb.Events.Native;

namespace Usb.Events;

/// <summary>
/// Main Usb.Events interface
/// </summary>
public interface IUsbEventWatcher : IDisposable
{
    Task<IReadOnlyCollection<UsbDevice>> GetUsbDevicesAsync(CancellationToken  cancellationToken = default);
    event EventHandler<UsbDeviceEvent>? OnUsbDeviceMounted;
    event EventHandler<UsbDeviceEvent>? OnUsbDeviceEjected;
    event EventHandler<UsbDeviceEvent>? OnUsbDeviceAdded;
    event EventHandler<UsbDeviceEvent>? OnUsbDeviceRemoved;
    Task StartAsync(StartingBehavior startingBehavior, UserData userData, CancellationToken cancellationToken = default);
    Task StopAsync();
}
