using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Usb.Events.Models;

namespace Usb.Events.Native.Linux.SystemD;

[SupportedOSPlatform("linux")]
internal partial class SdDeviceMonitor() : SafeHandleZeroOrMinusOneIsInvalid(true), IUsbEventProducer
{
    private static partial class NativeMethods
    {
        private const string LibName = "libsystemd";

#if NET7_0_OR_GREATER
        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial SdDeviceMonitor sd_device_monitor_unref(SdDeviceMonitor monitor);

        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_monitor_new(out SdDeviceMonitor ret);

        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_monitor_filter_add_match_subsystem_devtype(SdDeviceMonitor monitor,
            string subsystem, string? devtype);

        // Use with System.Net.Sockets for async monitoring
        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_monitor_get_fd(SdDeviceMonitor monitor);

        // Instead of start(), we manually extract devices when notified by the socket
        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_monitor_receive(SdDeviceMonitor monitor, out SdDevice device);
#else
        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_monitor_new(out SdDeviceMonitor ret);

        [DllImport(LibName)]
        internal static extern SdDeviceMonitor sd_device_monitor_unref(SdDeviceMonitor monitor);

        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_monitor_filter_add_match_subsystem_devtype(SdDeviceMonitor monitor,
            string subsystem, string? devtype);

        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_monitor_get_fd(SdDeviceMonitor monitor);

        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_monitor_receive(SdDeviceMonitor monitor, out SdDevice device);
#endif
    }

    public static SdDeviceMonitor Create(IEnumerable<string> subsystems)
    {
        int result = NativeMethods.sd_device_monitor_new(out SdDeviceMonitor monitor);

        if (result < 0)
            throw new InvalidOperationException(
                "Failed to create new systemd device monitor instance.",
                new Win32Exception(-result));

        foreach (var subsystem in subsystems)
        {
            result = NativeMethods.sd_device_monitor_filter_add_match_subsystem_devtype(monitor, subsystem, null);

            if (result < 0)
                throw new InvalidOperationException(
                    $"Failed to add subsystem {subsystem} for device monitoring.",
                    new Win32Exception(-result));
        }

        return monitor;
    }

    public async Task ProduceAsync(ChannelWriter<UsbDeviceEvent> writer, UserData userData, CancellationToken cancellationToken)
    {
        int result = NativeMethods.sd_device_monitor_get_fd(this);

        if (result < 0)
            throw new InvalidOperationException(
                "Failed to get systemd device monitor file handle",
                new Win32Exception(-result));

#if NET7_0_OR_GREATER
        using SafeSocketHandle socketHandle = new(result, ownsHandle: false);
        Socket managedSocket = new(socketHandle);
        Memory<byte> dummyBuffer = new(new byte[1]);
#endif

        while (!cancellationToken.IsCancellationRequested)
        {
#if NET7_0_OR_GREATER
            await managedSocket.ReceiveAsync(dummyBuffer, SocketFlags.Peek, cancellationToken);
#else
            await Epoll.WaitAsync(result, cancellationToken);
#endif

            if (cancellationToken.IsCancellationRequested) break;

            while (NativeMethods.sd_device_monitor_receive(this, out var device) > 0)
            {
                if (device.IsInvalid) continue;

                using var sdDevice = device;

                await writer.WriteAsync(new UsbDeviceEvent()
                {
                    Action = sdDevice.GetAction(),
                    Context = userData,
                    Device = sdDevice.GetDeviceData()
                }, cancellationToken);
            }
        }
    }

    protected override bool ReleaseHandle()
    {
        return NativeMethods.sd_device_monitor_unref(this).IsInvalid;
    }
}
