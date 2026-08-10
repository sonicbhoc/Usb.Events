using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Usb.Events.Linux;
using Usb.Events.Models;

namespace Usb.Events.Native.Linux.SystemD;

[SupportedOSPlatform("linux")]
internal partial class SdDeviceEnumerator() : SafeHandleZeroOrMinusOneIsInvalid(true), IEnumerator<UsbDevice>
{
    private static partial class NativeMethods
    {
        private const string LibName = "libsystemd";

#if NET7_0_OR_GREATER
        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_enumerator_new(out SdDeviceEnumerator ret);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial SdDeviceEnumerator sd_device_enumerator_unref(SdDeviceEnumerator enumerator);

        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_enumerator_add_match_parent(SdDeviceEnumerator enumerator,
            SdDevice parent);

        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_enumerator_add_match_subsystem(SdDeviceEnumerator enumerator,
            string subsystem, int match);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial SdDeviceUnowned sd_device_enumerator_get_device_first(SdDeviceEnumerator enumerator);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial SdDeviceUnowned sd_device_enumerator_get_device_next(SdDeviceEnumerator enumerator);
#else
        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_enumerator_new(out SdDeviceEnumerator ret);

        [DllImport(LibName)]
        internal static extern SdDeviceEnumerator sd_device_enumerator_unref(SdDeviceEnumerator enumerator);

        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_enumerator_add_match_parent(SdDeviceEnumerator enumerator,
            SdDeviceUnowned parent);

        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_enumerator_add_match_subsystem(SdDeviceEnumerator enumerator,
            string subsystem, int match);

        [DllImport(LibName)]
        internal static extern SdDeviceUnowned sd_device_enumerator_get_device_first(SdDeviceEnumerator enumerator);

        [DllImport(LibName)]
        internal static extern SdDeviceUnowned sd_device_enumerator_get_device_next(SdDeviceEnumerator enumerator);
#endif
    }

    public static SdDeviceEnumerator Create(IEnumerable<string> subsystems)
    {
        int result = NativeMethods.sd_device_enumerator_new(out SdDeviceEnumerator enumerator);

        if (result < 0)
        {
            throw new InvalidOperationException(
                "Could not initialize systemd device enumerator.",
                new Win32Exception(-result));
        }

        foreach (string subsystem in subsystems)
        {
            result = NativeMethods.sd_device_enumerator_add_match_subsystem(enumerator, subsystem, 1);

            if (result < 0)
            {
                throw new InvalidOperationException(
                    $"Could not match subsystem {subsystem} for monitoring.",
                    new Win32Exception(-result));
            }
        }

        return enumerator;
    }

    public UsbDevice Current { get; private set; } = null!;
    private SdDeviceUnowned _currentNative = new();

    object IEnumerator.Current => Current;

    private bool _isStarted;

    protected override bool ReleaseHandle()
    {
        return NativeMethods.sd_device_enumerator_unref(this).IsInvalid;
    }

    public bool MoveNext()
    {
        if (!_isStarted)
        {
            _currentNative = NativeMethods.sd_device_enumerator_get_device_first(this);
            _isStarted = true;
        }
        else
        {
            _currentNative = NativeMethods.sd_device_enumerator_get_device_next(this);
        }

        if (_currentNative.IsInvalid) return false;

        Current = _currentNative.GetDeviceData();

        return true;
    }

    public void Reset()
    {
        Current = new();
        _isStarted = false;
    }
}

[SupportedOSPlatform("linux")]
internal class SystemdDeviceList(IEnumerable<string> subsystems) : IEnumerable<UsbDevice>, IDisposable, IUsbEventProducer
{
    private readonly SdDeviceEnumerator _enumerator = SdDeviceEnumerator.Create(subsystems);

    public IEnumerator<UsbDevice> GetEnumerator()
    {
        return _enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _enumerator.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task ProduceAsync(ChannelWriter<UsbDeviceEvent> writer, UserData userData,
        CancellationToken cancellationToken)
    {
        foreach (var device in this)
        {
            await writer.WriteAsync(new UsbDeviceEvent
            {
                Action = UsbDeviceAction.Plugged,
                Context = userData,
                Device = device
            }, cancellationToken);
        }
    }
}
