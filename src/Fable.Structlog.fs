module Fable.Logging.Structlog

open System
open System.Collections.Generic

open Fable.Core


[<Import("FilteringBoundLogger", "structlog.types")>]
type FilteringBoundLogger =
    [<Emit("$0.debug($1, **$2)")>]
    abstract Debug: event: string * kw: IDictionary<string, obj> -> unit

    [<Emit("$0.info($1, **$2)")>]
    abstract Info: event: string * kw: IDictionary<string, obj> -> unit

    [<Emit("$0.warning($1, **$2)")>]
    abstract Warning: event: string * kw: IDictionary<string, obj> -> unit

    [<Emit("$0.error($1, **$2)")>]
    abstract Error: event: string * kw: IDictionary<string, obj> -> unit

    [<Emit("$0.fatal($1, **$2)")>]
    abstract Fatal: event: string * kw: IDictionary<string, obj> -> unit

    [<Emit("$0.exception($1, **$2)")>]
    abstract Exception: exn * kw: IDictionary<string, obj> -> unit

    [<Emit("$0.critical($1, **$2)")>]
    abstract Critical: event: string * kw: IDictionary<string, obj> -> unit

type WrappedLogger = obj
type EventDict = Dictionary<string, obj>

type Processor = Func<WrappedLogger, string, EventDict, EventDict>

type StructLog =
    abstract member getLogger: unit -> FilteringBoundLogger
    abstract member wrapLogger: FilteringBoundLogger * ResizeArray<Processor> -> FilteringBoundLogger

[<ImportAll("structlog")>]
let structLog: StructLog = nativeOnly

type ConsoleLogger() =

    let wrappedLogger = structLog.getLogger ()

    let processor (logger: WrappedLogger) (logMethod: string) (eventDict: EventDict) : EventDict =
        let event = eventDict["event"] :?> string
        let args = eventDict["args"] :?> obj[]

        let format, parameters = Common.translateFormat event args

        // printfn "string.format %A" (format, args)
        let event = String.Format(format, args = args)
        parameters["event"] <- event
        parameters

    let logger =
        structLog.wrapLogger (wrappedLogger, [ Processor(processor) ] |> ResizeArray)

    interface ILogger with
        member this.Log(state: LogState) =
            let message = state.Format
            let args = state.Args
            // let error = state.Exception
            let level = state.Level

            match level with
            | LogLevel.Debug -> logger.Debug(message, dict [ "args", args ])
            | LogLevel.Information -> logger.Info(message, dict [ "args", args ])
            | LogLevel.Warning -> logger.Warning(message, dict [ "args", args ])
            | LogLevel.Error -> logger.Error(message, dict [ "args", args ])
            | LogLevel.Critical -> logger.Critical(message, dict [ "args", args ])
            | _ -> logger.Info(message, dict [ "args", args ])

        member this.IsEnabled(logLevel: LogLevel) = true
        member this.BeginScope(var0) = failwith "Not implemented"


type ConsoleLoggerProvider() =
    interface ILoggerProvider with
        member this.CreateLogger(name) = new ConsoleLogger()
        member this.Dispose() = ()

// module Test =
//     let logger = ConsoleLogger()
//     logger.LogDebug("Hello {name}", "World")
