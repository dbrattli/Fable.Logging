namespace Fable.Logging

open System

type ConsoleLogger(name: string) =
    interface ILogger with
        member this.Log(state: LogState) =
            let message, _ = Common.translateFormat name state.Format state.Args

            let message =
                match state.Exception with
                | Some error ->
                    message
                    + Environment.NewLine
                    + error.ToString()
                | _ -> message

            Console.WriteLine(message)

        member this.IsEnabled(logLevel: LogLevel) = true
        member this.BeginScope(_) = failwith "Not implemented"
