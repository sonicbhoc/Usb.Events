module internal Usb.Events.Net.Core.Events

open System
open FsToolkit.ErrorHandling

type internal Aggregate<'state, 'command, 'event, 'error> =
    { Execute: 'state -> 'command -> Result<'event list, 'error list>
      Evolve: 'state -> 'event -> 'state
      InitialState: 'state }

    member this.Fold state events = events |> List.fold this.Evolve state
    member this.Rehydrate events = this.Fold this.InitialState events

type State =
    { Config: ServiceConfig
      Devices: Map<DeviceSystemPath, UsbDevice>
      MountPoints: Map<UsbDevice, DeviceMountPath>
      MountEvents: MountEvent IObservable option
      PlugEvents: PlugEvent IObservable option
      IsRunning: bool }

let initialState =
    { Config =
        { IncludeAlreadyPresentDevices = false
          UsePnPEntity = false
          IncludeTTY = false }
      Devices = Map.empty
      MountPoints = Map.empty
      IsRunning = false
      MountEvents = None
      PlugEvents = None }

type Command =
    | Start of ServiceConfig
    | Stop
    | ReceiveMountEvent of event: MountEvent
    | ReceivePlugEvent of event: PlugEvent

type Event =
    | Started of IUsbEventWatcher
    | Stopped
    | MountEventDetected of MountEvent
    | PlugEventDetected of PlugEvent

type Error =
    | ServiceAlreadyRunning
    | ServiceNotRunning
    | DuplicatePlugEvent of PlugEvent
    | DeviceNotMounted of DeviceSystemPath * DeviceMountPath

module OnlyIf =
    let serviceIsRunning state =
        match state.IsRunning with
        | true -> Validation.ok state
        | false -> Validation.error ServiceNotRunning

    let serviceIsNotRunning state =
        match state.IsRunning with
        | true -> Validation.error ServiceAlreadyRunning
        | false -> Validation.ok state

    let plugEventIsUnique state plugEvent =
        match plugEvent with
        | UsbDeviceAdded(sysPath, _)
        | UsbDeviceRemoved sysPath when state.Devices.ContainsKey sysPath ->
            DuplicatePlugEvent plugEvent |> Validation.error
        | _ -> Validation.ok state

    let deviceMounted state mountEvent =
        match mountEvent with
        | UsbDriveEjected(sysPath, mountPath) when not <| state.Devices.ContainsKey sysPath ->
            DeviceNotMounted(sysPath, mountPath) |> Validation.error
        | _ -> Validation.ok state
