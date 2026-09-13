using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Usb.Events.Models;

namespace Usb.Events.Native.Linux.SystemD;

[SupportedOSPlatform("linux")]
internal class SdDeviceList(IEnumerable<string> subsystems) : IEnumerable<UsbDevice>, IDisposable
{
    private readonly SdDeviceEnumerator _enumerator = SdDeviceEnumerator.Create(subsystems);

    public IEnumerator<UsbDevice> GetEnumerator()
    {
        return _enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _enumerator.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public ValueTask<IEnumerable<UsbDeviceEvent>> ScanAsync(CancellationToken ct)
    {
        return ValueTask.FromResult(this.Select(device => new UsbDeviceEvent()
        {
            Action = UsbDeviceAction.Plugged,
            Context = null,
            Device = device
        }));
    }
}
