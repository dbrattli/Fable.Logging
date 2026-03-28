namespace Fable.Logging

open System

type ConsoleLogger(name: string) =
    interface ILogger with
        member this.Log(state: LogState) =
            let message = state.Format
            let args = state.Args
            let error = state.Exception
            let level = state.Level

            let message =
                match args with
                | null
                | [||] -> message
                | _ -> String.Format(message, args)

            let message =
                let message, _ = Common.translateFormat name message args

                match error with
                | Some error ->
                    message
                    + Environment.NewLine
                    + error.ToString()
                | _ -> message

            Console.WriteLine(message)

        member this.IsEnabled(logLevel: LogLevel) = true
        member this.BeginScope(var0) = failwith "Not implemented"
