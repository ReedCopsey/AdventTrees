namespace TreeLogic.Model

type Location = { X: float; Y: float }

type Tree = { Position : Location ; Height : float ; Decorated : bool }

type Forest = { Trees : Tree list }
with
    static member Empty with get() = { Trees = [] }
    member f.Add tree = 
        { Trees = tree :: f.Trees }
    member f.Decorate tree = 
        let existing = f.Trees
        let updated = 
            existing
            |> List.except [ tree ]
        { Trees = { tree with Decorated = true } :: updated }
    member f.Prune max = 
        let updated = 
            if max < List.length f.Trees then
                f.Trees
                |> List.take max
            else
                f.Trees
        { Trees = updated }

type ForestUpdate =
    | Add of Tree * Forest
    | Decorate of Tree * Forest

type ForestUpdateResult =
    | Success of Forest
    | Pruned of Forest
    | Error of string

module ForestManager =
    let private update forest f (reporter : MailboxProcessor<ForestUpdateResult>) =
        let updated = f forest
        Success updated |> reporter.Post
        if List.length updated.Trees > 10 then
            updated.Prune 5 |> Pruned |> reporter.Post
    let createUpdateAgent reporter =
        let updater (inbox : MailboxProcessor<ForestUpdate>) =
            let rec loop() =
                async {
                    let! forestUpdate = inbox.Receive()
                    let f, forest = 
                        match forestUpdate with
                        | Add(tree, forest) -> (fun _ -> forest.Add tree), forest
                        | Decorate(tree, forest) -> (fun _ -> forest.Decorate tree), forest
                    update forest f reporter
                    do! loop()
                }
            loop()
        let result = new MailboxProcessor<ForestUpdate>(updater)
        result.Start()
        result
