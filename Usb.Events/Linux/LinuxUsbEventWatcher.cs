using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Usb.Events.Models;
using Usb.Events.Native;

namespace Usb.Events.Linux;

public class LinuxUsbEventWatcher(IUsbEventStream stream) : IUsbEventWatcher
{
    public Task<IReadOnlyCollection<UsbDevice>> GetUsbDevicesAsync(CancellationToken cancellationToken = default)
        => stream.GetUsbDevicesAsync(cancellationToken);

    public Task<IReadOnlyDictionary<UsbDevice, MountPoint>> GetMountPointsAsync(
        CancellationToken cancellationToken = default)
        => stream.GetMountPointsAsync(cancellationToken);

    public event EventHandler<UsbDeviceEvent>? OnUsbDeviceMounted;
    public event EventHandler<UsbDeviceEvent>? OnUsbDeviceEjected;
    public event EventHandler<UsbDeviceEvent>? OnUsbDeviceAdded;
    public event EventHandler<UsbDeviceEvent>? OnUsbDeviceRemoved;

    public bool IsRunning => stream.IsRunning;

    public

    public async Task StartAsync(UserData userData, CancellationToken cancellationToken = default)
    {
        await stream.StartAsync(userData, cancellationToken);
    }

    public async Task StopAsync()
    {
        await stream.StopAsync();
    }
}
