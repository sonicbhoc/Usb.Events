namespace Usb.Events.Models;

public record UsbEventWatcherOptions(StartingBehavior StartingBehavior, int MaxBufferedEvents = 100)
{
    public static int SuggestedDefaultBufferSize = 100;
    public static int SuggestedHighThroughputBufferSize = 1000;
};
