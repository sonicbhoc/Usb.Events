namespace Usb.Events.Net.Core

open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

type MountEvent =
    | UsbDriveMounted of DeviceSystemPath * DeviceMountPath
    | UsbDriveEjected of DeviceSystemPath * DeviceMountPath

type PlugEvent =
    | UsbDeviceAdded of DeviceSystemPath * UsbDevice
    | UsbDeviceRemoved of DeviceSystemPath

type ServiceConfig =
    { IncludeAlreadyPresentDevices: bool
      UsePnPEntity: bool
      IncludeTTY: bool }

type IUsbService =
    inherit IDisposable
    abstract member Start: ServiceConfig -> unit

    abstract member Stop: unit -> unit

type IUsbEventWatcher =
    inherit IUsbService
    abstract member UsbDevices: IDictionary<DeviceSystemPath, UsbDevice>
    abstract member MountPaths: IDictionary<UsbDevice, DeviceMountPath>
    abstract member MountEvents: MountEvent IObservable
    abstract member PlugEvents: PlugEvent IObservable
    abstract member IsRunning: bool

type ISystemMonitor =
    inherit IUsbService
    abstract member IsMonitoring: bool

type ISystemMonitorFactory =
    abstract member Create:
        plugEventCallback: Action<PlugEvent> *
        mountEventCallback: Action<MountEvent> *
        startCallback: Action<IReadOnlyCollection<UsbDevice>> ->
            ISystemMonitor

    abstract member Create:
        plugEventCallback: Action<PlugEvent> * mountEventCallback: Action<MountEvent> -> ISystemMonitor
