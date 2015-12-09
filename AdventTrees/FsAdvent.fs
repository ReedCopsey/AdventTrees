namespace FsAdvent

open System.Windows
open System.Windows.Controls
open System.Windows.Input

open FsXaml
open TreeLogic.ViewModels


type MainWindow = XAML<"MainWindow.xaml">


module internal MouseConverters =
    let addConverter (args : MouseEventArgs) =
        match args.OriginalSource with
        | :? Canvas ->
            // Only add to Canvas elements
            let source = args.OriginalSource :?> IInputElement
            let pt = args.GetPosition(source)
            Add { X = pt.X; Y = pt.Y }
        | _ -> 
            Unknown

    let decorateConverter (args : MouseEventArgs) =
        let source = args.OriginalSource :?> FrameworkElement
        let tree = unbox source.DataContext
        Decorate tree

type AddConverter() = inherit EventArgsConverter<MouseEventArgs, TreeEvent>(MouseConverters.addConverter, TreeEvent.Unknown)
type DecorateConverter() = inherit EventArgsConverter<MouseEventArgs, TreeEvent>(MouseConverters.decorateConverter, TreeEvent.Unknown)