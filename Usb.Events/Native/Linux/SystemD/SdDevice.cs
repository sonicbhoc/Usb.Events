using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;
using Usb.Events.Models;

namespace Usb.Events.Native.Linux.SystemD;

[SupportedOSPlatform("linux")]
internal partial class SdDevice : SafeHandleZeroOrMinusOneIsInvalid
{
    private static partial class NativeMethods
    {
        private const string LibName = "libsystemd";

#if NET7_0_OR_GREATER
        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_get_property_value(SdDevice device, string key, ref string? value);

        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_get_syspath(SdDevice device, ref string? path);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial SdDevice sd_device_unref(SdDevice device);

        [LibraryImport(LibName, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int sd_device_get_action(SdDevice device, out SdDeviceAction action);
#else
        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_get_property_value(SdDevice device, string key, ref string? value);

        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_get_syspath(SdDevice device, ref string? path);

        [DllImport(LibName)]
        internal static extern SdDevice sd_device_unref(SdDevice device);

        [DllImport(LibName, SetLastError = true)]
        internal static extern int sd_device_get_action(SdDevice device, out SdDeviceAction action);
#endif
    }

    public SdDevice() : this(true) { }

    protected SdDevice(bool ownsHandle) : base(ownsHandle) => SetHandleAsInvalid();

    protected override bool ReleaseHandle() => NativeMethods.sd_device_unref(this).IsInvalid;

    public UsbDeviceAction GetAction()
    {
        int result = NativeMethods.sd_device_get_action(this, out var action);

        if (result > 0)
            throw new InvalidOperationException(
                "Unable to determine device action.",
                new Win32Exception(-result));

        return action.ToDeviceAction();
    }

    public UsbDevice GetDeviceData()
    {
        if (IsInvalid)
            throw new InvalidOperationException("Device state is invalid; unable to extract property values.");

        // Get properties for UsbDevice from systemd context
        string? name = null;
        string? sysPath = null;
        string? productName = null;
        string? productDescription = null;
        string? productId = null;
        string? serialNumber = null;
        string? vendorName = null;
        string? vendorDescription = null;
        string? vendorId = null;

        _ = NativeMethods.sd_device_get_property_value(this, "DEVNAME", ref name);
        _ = NativeMethods.sd_device_get_property_value(this, "ID_MODEL", ref productName);
        _ = NativeMethods.sd_device_get_property_value(this, "ID_MODEL_FROM_DATABASE", ref productDescription);
        _ = NativeMethods.sd_device_get_property_value(this, "ID_MODEL_ID", ref productId);
        _ = NativeMethods.sd_device_get_property_value(this, "ID_SERIAL_SHORT", ref serialNumber);
        _ = NativeMethods.sd_device_get_property_value(this, "ID_VENDOR", ref vendorName);
        _ = NativeMethods.sd_device_get_property_value(this, "ID_VENDOR_FROM_DATABASE", ref vendorDescription);
        _ = NativeMethods.sd_device_get_property_value(this, "ID_VENDOR_ID", ref vendorId);

        int result = NativeMethods.sd_device_get_syspath(this, ref sysPath);
        if (result > 0 || sysPath is null)
            throw new InvalidOperationException(
                "Unable to retreive SysPath for Device",
                new Win32Exception(-result));

        return new UsbDevice(new(sysPath))
        {
            DeviceName = name ?? string.Empty,
            MountedDirectoryPath = string.Empty,
            Product = productName ?? string.Empty,
            ProductDescription = productDescription ?? string.Empty,
            ProductId = productId ?? string.Empty,
            SerialNumber = serialNumber ?? string.Empty,
            Vendor = vendorName ?? string.Empty,
            VendorDescription = vendorDescription ?? string.Empty,
            VendorId = vendorId ?? string.Empty,
            IsMounted = false,
            IsEjected = false
        };
    }
}
