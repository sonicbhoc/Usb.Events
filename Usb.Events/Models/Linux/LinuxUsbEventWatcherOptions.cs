namespace Usb.Events.Models.Linux;

public record LinuxUsbEventWatcherOptions(StartingBehavior StartingBehavior, int MaxBufferedEvents = 100, bool IncludeTtySubsystem = false)
    : UsbEventWatcherOptions(StartingBehavior, MaxBufferedEvents) { }
