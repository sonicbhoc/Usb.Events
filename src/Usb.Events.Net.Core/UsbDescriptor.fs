namespace Usb.Events.Net.Core

[<Struct>]
type ByteId = ByteId of id: uint16

[<RequireQualifiedAccess>]
module ByteId =
    let ofString (str: string) =
        let str: string =
            match str.StartsWith "0x" with
            | true -> str.Substring 2
            | false -> str

        System.UInt16.Parse(str, System.Globalization.NumberStyles.HexNumber)

    let toString (ByteId id) = id.ToString("X")

[<Struct>]
type DeviceName = DeviceName of name: string option

[<RequireQualifiedAccess>]
module DeviceName =
    let ofString (str: string option) =
        match str with
        | None -> None
        | Some s when System.String.IsNullOrWhiteSpace s -> None
        | Some s -> Some s
        |> DeviceName

[<Struct>]
type DeviceSystemPath = DeviceSystemPath of path: string option

[<RequireQualifiedAccess>]
module DeviceSystemPath =
    let ofString (str: string option) =
        match str with
        | None -> None
        | Some s when System.String.IsNullOrWhiteSpace s -> None
        | Some s -> Some s
        |> DeviceSystemPath

[<Struct>]
type DeviceMountPath = DeviceMountPath of mountPath: string option

[<RequireQualifiedAccess>]
module DeviceMountPath =
    let ofString (str: string option) =
        match str with
        | None -> None
        | Some s when System.String.IsNullOrWhiteSpace s -> None
        | Some s -> Some s
        |> DeviceMountPath

[<Struct>]
type SerialNumber = SerialNumber of sn: string

[<Struct>]
type Product =
    { Name: string
      Id: ByteId
      Description: string }

[<Struct>]
type Vendor =
    { Name: string
      Id: ByteId
      Description: string }

type UsbDevice =
    { Name: DeviceName
      Product: Product
      SerialNumber: SerialNumber
      Vendor: Vendor
      IsMounted: bool
      IsEjected: bool }
