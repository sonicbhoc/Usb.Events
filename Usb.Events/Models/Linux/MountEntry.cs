using System.Runtime.Versioning;

namespace Usb.Events.Models.Linux;

[SupportedOSPlatform("linux")]
internal record MountEntry(
    DeviceNode Device,
    MountPoint MountPoint,
    string FileSystemType,
    string Options,
    int DumpFrequency,
    int PassNumber);
