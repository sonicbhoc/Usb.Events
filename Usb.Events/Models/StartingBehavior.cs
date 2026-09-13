using System;

namespace Usb.Events.Models;

[Flags]
public enum StartingBehavior
{
    StartImmediately,
    IncludeAttached,
}
