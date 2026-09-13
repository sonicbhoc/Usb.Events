namespace Usb.Events.Models;

public readonly record struct MountPoint(string? Value)
{
    public static MountPoint NotMounted { get; } = new(null);

    public bool IsMounted => Value != null;
}
