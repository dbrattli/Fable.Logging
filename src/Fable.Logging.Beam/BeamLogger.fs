module Fable.Logging.Beam

open Fable.Beam

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

type LoggerProvider() =
    interface ILoggerProvider with
        member _.CreateLogger(name) = Logger(name)
        member _.Dispose() = ()
