using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Usb.Events.Native;

public class UserData : SafeHandleZeroOrMinusOneIsInvalid
{
    public static UserData Empty => new(null);

    // Required default constructor for some marshaling scenarios
    protected UserData() : base(ownsHandle: true)
    {
    }

    public UserData(object? value) : base(ownsHandle: true)
    {
        if (value is null)
        {
            SetHandleAsInvalid();
            return;
        }

        // Allocates a normal handle. This keeps the object alive
        // and gives us a stable IntPtr token for native callbacks.
        var gcHandle = GCHandle.Alloc(value, GCHandleType.Normal);

        // Set the base SafeHandle's handle to the GCHandle's internal pointer
        SetHandle(GCHandle.ToIntPtr(gcHandle));
    }

    // Safely extracts the managed object from the internal handle pointer
    public object? Target
    {
        get
        {
            if (IsInvalid) return null;
            GCHandle gcHandle = GCHandle.FromIntPtr(handle);
            return gcHandle.IsAllocated ? gcHandle.Target : null;
        }
    }

    protected override bool ReleaseHandle()
    {
        GCHandle gcHandle = GCHandle.FromIntPtr(handle);
        if (gcHandle.IsAllocated)
        {
            gcHandle.Free();
        }
        return true;
    }

    public UserData<T>? Cast<T>() where T : class => this as UserData<T>;
}

public class UserData<T> : UserData where T : class
{
    private UserData() : base() { }

    public UserData(T? value) : base(value)
    {
    }

    public new T? Target => base.Target as T;
}
