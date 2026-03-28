module Fable.Logging.Structlog

open System
open System.Collections.Generic

open Fable.Core


[<Import("FilteringBoundLogger", "structlog.types")>]
type FilteringBoundLogger =
    [<Emit("$0.bind(**($1 or {}))")>]
    abstract Bind: ?newValues: IDictionary<string, obj> -> FilteringBoundLogger

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
    abstract Exception: event: string * kw: IDictionary<string, obj> -> unit

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

type Logger(name: string, processors: Processor list) =

    let wrappedLogger = structLog.getLogger().Bind()

    // processor that combines args with the placeholders in the format
    // string to generate parameters to be used with structlog
    let processor (_: WrappedLogger) (_: string) (eventDict: EventDict) : EventDict =
        let event = eventDict["event"] :?> string
        let args = eventDict["args"] :?> obj[]

        let _, parameters = Common.translateFormat name event args
        parameters

    let logger =
        structLog.wrapLogger (
            wrappedLogger,
            Processor(processor) :: processors
            |> ResizeArray
        )

    new(name: string) = Logger(name, [])

    member val MinimumLevel = LogLevel.Trace with get, set

    interface ILogger with
        member x.Log(state: LogState) =
            let level = state.Level

            if level >= x.MinimumLevel then
                let message = state.Format
                let args = state.Args
                let error = state.Exception

                match level with
                | LogLevel.Debug -> logger.Debug(message, dict [ "args", args ])
                | LogLevel.Information -> logger.Info(message, dict [ "args", args ])
                | LogLevel.Warning -> logger.Warning(message, dict [ "args", args ])
                | LogLevel.Error ->
                    match error with
                    | Some ex -> logger.Exception(message, dict [ "args", args ])
                    | None -> logger.Error(message, dict [ "args", args ])
                | LogLevel.Critical -> logger.Critical(message, dict [ "args", args ])
                | _ -> logger.Info(message, dict [ "args", args ])

        member x.IsEnabled(logLevel: LogLevel) = logLevel >= x.MinimumLevel
        member x.BeginScope(var0) = failwith "Not implemented"


type ConsoleLogger(name) =
    inherit Logger(name, [])

[<Import("JSONRenderer", "structlog.processors")>]
let JSONRenderer: unit -> Processor = nativeOnly

type JsonLogger(name) =
    inherit Logger(name = name, processors = [ JSONRenderer() ])


type ConsoleLoggerProvider() =
    interface ILoggerProvider with
        member this.CreateLogger(name) = ConsoleLogger(name)
        member this.Dispose() = ()

type JsonLoggerProvider() =
    interface ILoggerProvider with
        member this.CreateLogger(name) = JsonLogger(name)
        member this.Dispose() = ()
