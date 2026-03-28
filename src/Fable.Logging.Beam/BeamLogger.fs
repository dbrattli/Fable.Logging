module Fable.Logging.Beam

open Fable.Core
open Fable.Beam

[<Emit("logger:set_primary_config(level, $0)")>]
let private setPrimaryLevel (_level: obj) : unit = nativeOnly

let private toErlangLevel (level: LogLevel) =
    match level with
    | LogLevel.Trace
    | LogLevel.Debug -> "debug"
    | LogLevel.Information -> "info"
    | LogLevel.Warning -> "warning"
    | LogLevel.Error -> "error"
    | LogLevel.Critical -> "critical"
    | _ -> "info"

type Logger(name: string) =

    member val MinimumLevel = LogLevel.Trace with get, set

    interface ILogger with
        member x.Log(state: LogState) =
            let level = state.Level

            if level >= x.MinimumLevel then
                let message, _ = Common.translateFormat name state.Format state.Args

                match level with
                | LogLevel.Debug -> Logger.logger.debug (message)
                | LogLevel.Information -> Logger.logger.info (message)
                | LogLevel.Warning -> Logger.logger.warning (message)
                | LogLevel.Error -> Logger.logger.error (message)
                | LogLevel.Critical -> Logger.logger.critical (message)
                | _ -> Logger.logger.info (message)

        member x.IsEnabled(logLevel: LogLevel) = logLevel >= x.MinimumLevel
        member _.BeginScope(_) : System.IDisposable = failwith "Not implemented"

type LoggerProvider(?minimumLevel: LogLevel) =
    let level = defaultArg minimumLevel LogLevel.Trace

    do setPrimaryLevel (toErlangLevel level)

    interface ILoggerProvider with
        member _.CreateLogger(name) =
            let logger = Logger(name)
            logger.MinimumLevel <- level
            logger

        member _.Dispose() = ()
