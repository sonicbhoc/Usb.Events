#if !NET7_0_OR_GREATER // .NET 7 and greater clients can use the more efficient UNIX socket functionality in System.Net.Sockets
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Events.Native.Linux;

internal static class Epoll
{
    private const int EPOLL_CTL_ADD = 1;
    private const uint EPOLLIN = 0x001;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    private struct EpollEvent
    {
        [FieldOffset(0)] public uint Events;
        [FieldOffset(4)] public int Fd; // Using the fd as our data payload
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int epoll_create1(int flags);

    [DllImport("libc", SetLastError = true)]
    private static extern int epoll_ctl(int epfd, int op, int fd, ref EpollEvent ev);

    [DllImport("libc", SetLastError = true)]
    private static extern int epoll_wait(int epfd, [Out] EpollEvent[] events, int maxevents, int timeout);

    [DllImport("libc", SetLastError = true)]
    private static extern int eventfd(uint initval, int flags);

    [DllImport("libc", SetLastError = true)]
    private static extern int write(int fd, ref ulong buf, UIntPtr count);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);

    public static Task WaitAsync(int targetFd, CancellationToken ct)
    {
        // Create an OS-level signaling handle for cancellation
        int cancelFd = eventfd(0, 0);
        int epollFd = epoll_create1(0);

        if (epollFd < 0 || cancelFd < 0)
        {
            if (cancelFd >= 0) close(cancelFd);
            if (epollFd >= 0) close(epollFd);
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Register the device monitor and cancellation FD with epoll
        var evDevice = new EpollEvent { Events = EPOLLIN, Fd = targetFd };
        epoll_ctl(epollFd, EPOLL_CTL_ADD, targetFd, ref evDevice);

        var evCancel = new EpollEvent { Events = EPOLLIN, Fd = cancelFd };
        epoll_ctl(epollFd, EPOLL_CTL_ADD, cancelFd, ref evCancel);

        // Use epoll to raise cancellation events
        var registration = ct.Register(() =>
        {
            ulong signalValue = 1;
            write(cancelFd, ref signalValue, (nuint)sizeof(ulong));
        });

        // Offload the pure blocking OS wait to a separate thread pool context
        return Task.Run(() =>
        {
            try
            {
                var events = new EpollEvent[1];

                // Block infinitely with until a device event OR a cancellation signal hits
                int ready = epoll_wait(epollFd, events, 1, -1);

                if (ready > 0 && events[0].Fd == cancelFd)
                {
                    // The token triggered! Throw to match standard async semantics
                    throw new OperationCanceledException(ct);
                }
            }
            finally
            {
                // Clean up all resources safely
                registration.Dispose();
                close(epollFd);
                close(cancelFd);
            }
        }, ct);
    }
}
#endif
