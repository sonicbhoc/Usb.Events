using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Usb.Events.Models;
using Usb.Events.Native;

namespace Usb.Events.Linux;

internal class LinuxUsbEventStream(INativeUsbEventProducer nativeUsbEventMonitor, INativeUsbDeviceScanner? usbDeviceScanner) : IUsbEventStream
{
    private SemaphoreSlim CollectionSemaphore { get; } = new(1, 1);
    private List<UsbDevice> UsbDeviceEvents { get; } = new();
    private Dictionary<UsbDevice, MountPoint> MountPoints { get; } = new();
    private CancellationTokenSource? CancellationTokenSource { get; set; }

    public bool IsRunning => nativeUsbEventMonitor.IsRunning;

    public async Task<IReadOnlyCollection<UsbDevice>> GetUsbDevicesAsync(CancellationToken cancellationToken = default)
    {
        await CollectionSemaphore.WaitAsync(cancellationToken);

        try
        {
            return UsbDeviceEvents.AsReadOnly();
        }
        finally
        {
            CollectionSemaphore.Release();
        }
    }

    public async Task<IReadOnlyDictionary<UsbDevice, MountPoint>> GetMountPointsAsync(CancellationToken cancellationToken = default)
    {
        await CollectionSemaphore.WaitAsync(cancellationToken);

        try
        {
            return MountPoints.AsReadOnly();
        }
        finally
        {
            CollectionSemaphore.Release();
        }
    }

    public async IAsyncEnumerable<UsbDeviceEvent> MonitorAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
            throw new InvalidOperationException("Cannot monitor a stopped stream.");

        // First (if available), scan for existing devices; then monitor for new events indefinitely
        if (usbDeviceScanner is not null)
        {
            var devices = (await usbDeviceScanner.ScanAsync(cancellationToken)).ToList();

            foreach (var device in devices)
            {
                yield return device;
            }

            await CollectionSemaphore.WaitAsync(cancellationToken);

            try
            {
                UsbDeviceEvents.AddRange(devices.Select(d => d.Device));
            }
            finally
            {
                CollectionSemaphore.Release();
            }
        }

        await foreach(var device in nativeUsbEventMonitor.ProduceAsync(cancellationToken))
        {
            yield return device;

            await CollectionSemaphore.WaitAsync(cancellationToken);

            try
            {
                UsbDeviceEvents.Add(device.Device);
            }
            finally
            {
                CollectionSemaphore.Release();
            }
        }
    }

    public async Task StartAsync(UserData userData, CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
        {
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await nativeUsbEventMonitor.StartAsync(userData, cancellationToken);
        }
    }

    public async Task StopAsync()
    {
        if (IsRunning)
        {
            await nativeUsbEventMonitor.StopAsync();
        }
    }
}
