namespace Usb.Events.Models;

internal readonly record struct MountPoint(string? Value)
{
    public static MountPoint NotMounted { get; } = new(null);

    public bool IsMounted => Value != null;
}
