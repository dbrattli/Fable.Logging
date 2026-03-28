module Fable.Logging.JS

open Fable.Core

// fsharplint:disable MemberNames

[<Erase>]
type private IConsole =
    abstract log: msg: string -> unit
    abstract debug: msg: string -> unit
    abstract info: msg: string -> unit
    abstract warn: msg: string -> unit
    abstract error: msg: string -> unit

[<Emit("console")>]
let private console: IConsole = nativeOnly

type Logger(name: string) =

    member val MinimumLevel = LogLevel.Trace with get, set

    interface ILogger with
        member x.Log(state: LogState) =
            let level = state.Level

            if level >= x.MinimumLevel then
                let message, _ = Common.translateFormat name state.Format state.Args

                match level with
                | LogLevel.Trace -> console.debug (message)
                | LogLevel.Debug -> console.debug (message)
                | LogLevel.Information -> console.info (message)
                | LogLevel.Warning -> console.warn (message)
                | LogLevel.Error -> console.error (message)
                | LogLevel.Critical -> console.error (message)
                | _ -> console.log (message)

        member x.IsEnabled(logLevel: LogLevel) = logLevel >= x.MinimumLevel
        member _.BeginScope(_) : System.IDisposable = failwith "Not implemented"

type LoggerProvider() =
    interface ILoggerProvider with
        member _.CreateLogger(name) = Logger(name)
        member _.Dispose() = ()
