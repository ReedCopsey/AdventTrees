namespace TreeLogic.ViewModels

open System.Threading
open TreeLogic.Model
open FSharp.ViewModule

type TreeEvent =
    | Add of location:Location
    | Decorate of tree:Tree
    | Unknown

type ForestViewModel () as self =
    inherit EventViewModelBase<TreeEvent>()

    // Create a backing field for our Forest using FSharp.ViewModule
    let forest = self.Factory.Backing(<@ self.Forest @>, Forest.Empty)

    // This allows the processing to happen on other threads,
    // and avoids issues with collection updates from WPF
    let ui = SynchronizationContext.Current
    let rnd = System.Random()

    // Create an async update loop for our agent
    let update (inbox : MailboxProcessor<ForestUpdateResult>) =
        let rec loop() =
            async {
                let! update = inbox.Receive()
                match update with
                | Success updated ->
                    do! Async.SwitchToContext ui
                    forest.Value <- updated
                | Pruned updated ->
                    // Wait brief period (so you see the tree added before pruning), then update us
                    // Note: This creates a race condition if you click very fast
                    do! Async.Sleep 250
                    do! Async.SwitchToContext ui
                    forest.Value <- updated
                | Error _ -> 
                    // Handle error case here
                    ()
                do! loop()
            }
        loop()
    let reporter = new MailboxProcessor<_>(update)

    // Start our report handler
    do
        reporter.Start()

    // Create the agent used to update the model
    let updateAgent = ForestManager.createUpdateAgent reporter


    let handleEvent event =
        match event with
        | Add(location) -> 
            let height = 8.0 + rnd.NextDouble() * 4.0
            updateAgent.Post <| ForestUpdate.Add ({ Position = location ; Height = height ; Decorated = false }, forest.Value)                        
        | Decorate(tree) ->
            updateAgent.Post <| ForestUpdate.Decorate (tree, forest.Value)
        | Unknown -> 
            ()
    do
        self.EventStream
        |> Observable.subscribe handleEvent
        |> ignore

    // Bindings for our UI
    member __.Forest with get() = forest.Value
    member val MouseCommand = self.Factory.EventValueCommand()