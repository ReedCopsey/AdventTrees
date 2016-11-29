open System
open System.Windows


[<STAThread>]
[<EntryPoint>]
let main argv = 
    Application().Run(FsAdvent.MainWindow())
