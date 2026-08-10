using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Usb.Events.Models;

namespace Usb.Events;

internal interface IUsbEventConsumer
{
    Task ConsumeAsync(ChannelReader<UsbDeviceEvent> reader, CancellationToken cancellationToken);
}
