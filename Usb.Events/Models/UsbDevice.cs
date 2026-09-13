using System;

namespace Usb.Events.Models;

/// <summary>
/// USB device
/// </summary>
public record UsbDevice(DeviceNode DeviceSystemPath)
{
    /// <summary>
    /// Device name
    /// </summary>
    public string DeviceName { get; init; } = string.Empty;

    /// <summary>
    /// Device mounted directory path
    /// </summary>
    public string MountedDirectoryPath { get; init; } = string.Empty;

    /// <summary>
    /// Device product name
    /// </summary>
    public string Product { get; init; } = string.Empty;

    /// <summary>
    /// Device product description
    /// </summary>
    public string ProductDescription { get; init; } = string.Empty;

    /// <summary>
    /// Device product ID
    /// </summary>
    public string ProductId { get; init; } = string.Empty;

    /// <summary>
    /// Device serial number
    /// </summary>
    public string SerialNumber { get; init; } = string.Empty;

    /// <summary>
    /// Device vendor name
    /// </summary>
    public string Vendor { get; init; } = string.Empty;

    /// <summary>
    /// Device vendor description
    /// </summary>
    public string VendorDescription { get; init; } = string.Empty;

    /// <summary>
    /// Device vendor ID
    /// </summary>
    public string VendorId { get; init; } = string.Empty;

    /// <summary>
    /// Is device mounted
    /// </summary>
    public bool IsMounted { get; internal set; }

    /// <summary>
    /// Is device ejected
    /// </summary>
    public bool IsEjected { get; internal set; }

    /// <summary>
    /// Write all property values to a string
    /// </summary>
    /// <returns>Each property on a new line</returns>
    public override string ToString()
    {
        return "Device Name: " + DeviceName + Environment.NewLine +
               "Device System Path: " + DeviceSystemPath + Environment.NewLine +
               "Mounted Directory Path: " + MountedDirectoryPath + Environment.NewLine +
               "Product: " + Product + Environment.NewLine +
               "Product Description: " + ProductDescription + Environment.NewLine +
               "Product ID: " + ProductId + Environment.NewLine +
               "Serial Number: " + SerialNumber + Environment.NewLine +
               "Vendor: " + Vendor + Environment.NewLine +
               "Vendor Description: " + VendorDescription + Environment.NewLine +
               "Vendor ID: " + VendorId + Environment.NewLine;
    }
}
