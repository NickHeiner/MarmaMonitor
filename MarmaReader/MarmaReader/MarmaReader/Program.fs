open System
open System.Text.RegularExpressions

let SERVER_LOG = "server.log"

type event = Login | Logout

let parseLine line = 
    (* According to Minecraft.net, a username can only contain letters, numbers, and _ *)
    (* Sometimes we'll see a line of the form 
        2012-04-22 19:17:36 [INFO] /192.168.1.111:53084 lost connection
       But that only seems to happen after the server starts up, and doesn't seem to be relevant. *)
    let Match = Regex.Match (line, "(.*)\[INFO\] ([A-z_0-9]*) ((lost connection)|(.*logged in))")
    if not Match.Success 
    then None
    else 
        let groups = Match.Groups
        Some (groups.[2].Value, DateTime.Parse groups.[1].Value, if line.Contains("lost connection") then Logout else Login)

let updateEvents events ((name, time, change) as changeEvent) = Map.add name (time, change) events

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    IO.File.ReadLines <| IO.Path.Combine [|argv.[0];  SERVER_LOG|]
    |> Seq.map parseLine
    |> Seq.choose id
    |> Seq.fold updateEvents Map.empty
    |> printfn "%A"
    Console.ReadLine () |> ignore
    0 // return an integer exit code
