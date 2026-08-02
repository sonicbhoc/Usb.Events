using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace Usb.Events.Native.Linux;


[SupportedOSPlatform("linux")]
internal class sd_device_enumerator() : SafeHandleZeroOrMinusOneIsInvalid(true), IEnumerator<sd_device_unowned>
{
    public sd_device_unowned Current { get; private set; } = new();

    object IEnumerator.Current => Current;

    private bool _isStarted;

    protected override bool ReleaseHandle()
    {
        return libsystemd.sd_device_enumerator_unref(this).handle == IntPtr.Zero;
    }

    public bool MoveNext()
    {
        if (!_isStarted)
        {
            Current = libsystemd.sd_device_enumerator_get_device_first(this);
            _isStarted = true;
        }
        else
        {
            Current = libsystemd.sd_device_enumerator_get_device_next(this);
        }

        return !Current.IsInvalid;
    }

    public void Reset()
    {
        Current = new sd_device_unowned();
        _isStarted = false;
    }
}
