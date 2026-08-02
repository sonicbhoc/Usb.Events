using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Usb.Events.Native.Linux;

[SupportedOSPlatform("linux")]
// ReSharper disable once InconsistentNaming
internal static partial class libsystemd
{
#if NET7_0_OR_GREATER
    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial sd_device sd_device_ref(sd_device device);

    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial sd_device sd_device_unref(sd_device device);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_new_from_syspath(ref sd_device ret, string syspath);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_enumerator_new(ref sd_device_enumerator ret);

    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial sd_device_enumerator sd_device_enumerator_unref(sd_device_enumerator enumerator);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_enumerator_add_match_parent(sd_device_enumerator enumerator,
        sd_device parent);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_enumerator_add_match_subsystem(sd_device_enumerator enumerator,
        string subsystem, int match);

    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial sd_device_unowned sd_device_enumerator_get_device_first(sd_device_enumerator enumerator);

    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial sd_device_unowned sd_device_enumerator_get_device_next(sd_device_enumerator enumerator);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_get_property_value(sd_device device, string key, ref string value);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_get_syspath(sd_device device, ref string path);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_get_devname(sd_device device, ref string devname);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_get_devtype(sd_device device, ref string devtype);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_get_action(sd_device device, ref sd_device_action action);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_monitor_new(ref sd_device_monitor ret);

    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial sd_device_monitor sd_device_monitor_unref(sd_device_monitor monitor);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_monitor_filter_add_match_subsystem_devtype(sd_device_monitor monitor,
        string subsystem, string devtype);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_monitor_get_fd(sd_device_monitor monitor);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_monitor_receive(sd_device_monitor monitor, ref sd_device ret);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_monitor_attach_event(sd_device_monitor monitor, sd_event sdEvent);

    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_monitor_detach_event(sd_device_monitor monitor);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_monitor_start(sd_device_monitor monitor,
        sd_device_monitor_handler callback, IntPtr userdata);

    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_device_monitor_stop(sd_device_monitor monitor);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_event_new(ref sd_event ret);

    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial sd_event sd_event_unref(sd_event sdEvent);

    [LibraryImport(nameof(libsystemd), SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_event_loop(sd_event sdEvent);

    [LibraryImport(nameof(libsystemd), StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int sd_event_exit(sd_event sdEvent, int code);
#else
    [DllImport(nameof(libsystemd))]
    internal static extern sd_device sd_device_ref(sd_device device);

    [DllImport(nameof(libsystemd))]
    internal static extern sd_device sd_device_unref(sd_device device);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_new_from_syspath(ref sd_device ret, string syspath);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_enumerator_new(ref sd_device_enumerator ret);

    [DllImport(nameof(libsystemd))]
    internal static extern sd_device_enumerator sd_device_enumerator_unref(sd_device_enumerator enumerator);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_enumerator_add_match_parent(sd_device_enumerator enumerator,
        sd_device parent);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_enumerator_add_match_subsystem(sd_device_enumerator enumerator,
        string subsystem, int match);

    [DllImport(nameof(libsystemd))]
    internal static extern sd_device_unowned sd_device_enumerator_get_device_first(sd_device_enumerator enumerator);

    [DllImport(nameof(libsystemd))]
    internal static extern sd_device_unowned sd_device_enumerator_get_device_next(sd_device_enumerator enumerator);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_get_property_value(sd_device device, string key, ref string value);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_get_syspath(sd_device device, ref string path);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_get_devname(sd_device device, ref string devname);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_get_devtype(sd_device device, ref string devtype);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_get_action(sd_device device, ref sd_device_action action);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_monitor_new(ref sd_device_monitor ret);

    [DllImport(nameof(libsystemd))]
    internal static extern sd_device_monitor sd_device_monitor_unref(sd_device_monitor monitor);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_monitor_filter_add_match_subsystem_devtype(sd_device_monitor monitor,
        string subsystem, string devtype);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_monitor_get_fd(sd_device_monitor monitor);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_monitor_receive(sd_device_monitor monitor, ref sd_device ret);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_monitor_attach_event(sd_device_monitor monitor, sd_event sdEvent);

    [DllImport(nameof(libsystemd))]
    internal static extern int sd_device_monitor_detach_event(sd_device_monitor monitor);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_device_monitor_start(sd_device_monitor monitor,
        sd_device_monitor_handler callback, IntPtr userdata);

    [DllImport(nameof(libsystemd))]
    internal static extern int sd_device_monitor_stop(sd_device_monitor monitor);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_event_new(ref sd_event ret);

    [DllImport(nameof(libsystemd))]
    internal static extern sd_event sd_event_unref(sd_event sdEvent);

    [DllImport(nameof(libsystemd), SetLastError = true)]
    internal static extern int sd_event_loop(sd_event sdEvent);

    [DllImport(nameof(libsystemd))]
    internal static extern int sd_event_exit(sd_event sdEvent, int code);
#endif

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int sd_device_monitor_handler(sd_device_monitor_unowned monitor, sd_device_unowned device, IntPtr userdata);
}
