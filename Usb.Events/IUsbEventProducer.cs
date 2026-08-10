using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Usb.Events.Models;
using Usb.Events.Native;

namespace Usb.Events;

internal interface IUsbEventProducer
{
    Task ProduceAsync(ChannelWriter<UsbDeviceEvent> writer, UserData userData, CancellationToken cancellationToken);
}
