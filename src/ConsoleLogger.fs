namespace Fable.Logging
open System

type ConsoleLogger() =
    interface ILogger with
        member this.Log(state: LogState) =
            let message = state.Format
            let args = state.Args
            let error = state.Exception
            let level = state.Level
            let color =
                match level with
                | LogLevel.Trace -> ConsoleColor.Gray
                | LogLevel.Debug -> ConsoleColor.DarkGray
                | LogLevel.Information -> ConsoleColor.White
                | LogLevel.Warning -> ConsoleColor.Yellow
                | LogLevel.Error -> ConsoleColor.Red
                | LogLevel.Critical -> ConsoleColor.DarkRed
                | _ -> ConsoleColor.White
            let message =
                match args with
                | null | [||] -> message
                | _ -> String.Format(message, args)
            let message =
                let message, _ = Common.translateFormat message args
                match error with
                | Some error -> message + Environment.NewLine + error.ToString()
                | _ -> message
            //Console.ForegroundColor <- color
            Console.WriteLine(message)
            //Console.ResetColor()
        member this.IsEnabled(logLevel: LogLevel) = true
