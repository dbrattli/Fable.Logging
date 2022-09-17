namespace Fable.Logging

open System
open Fable.Core
open Fable.Core.PyInterop


type LogLevel =
    | Critical = 5
    | Debug = 1
    | Error = 4
    | Information = 2
    | None = 6
    | Trace = 0
    | Warning = 3

type LogState =
    {
        Level: LogLevel
        Format: string
        Args: obj array
        Exception: exn option
    }

    static member Create(level: LogLevel, format: string, ?parameters: obj array, ?error: exn) = {
        Level = level
        Format = format
        Args = defaultArg parameters [||]
        Exception = error
    }

type ILogger =
    abstract member Log: LogState -> unit
    abstract member IsEnabled: logLevel: LogLevel -> bool

type ILoggerProvider =
    inherit IDisposable

    abstract member CreateLogger: name: string -> ILogger

type ILoggerFactory =
    inherit IDisposable

    abstract member CreateLogger: name: string -> ILogger
    abstract member AddProvider: provider: ILoggerProvider -> unit

[<AutoOpen>]
module Extensions =
    type ILogger with

        member this.Log(logLevel: LogLevel, message: string) =
            if this.IsEnabled logLevel then
                LogState.Create(logLevel, message) |> this.Log

        member this.Log(logLevel: LogLevel, message: string, parameters: obj[]) =
            if this.IsEnabled logLevel then
                LogState.Create(logLevel, message, parameters) |> this.Log

        member this.Log(logLevel: LogLevel, error: exn, message: string) =
            if this.IsEnabled logLevel then
                LogState.Create(logLevel, message, error = error) |> this.Log

        member this.LogDebug(message: string, [<ParamArray>] parameters: obj[]) =
            if this.IsEnabled LogLevel.Debug then
                LogState.Create(LogLevel.Debug, message, parameters) |> this.Log

        member this.LogError(message: string, [<ParamArray>] parameters: obj[]) =
            if this.IsEnabled LogLevel.Error then
                LogState.Create(LogLevel.Error, message, parameters) |> this.Log

        member this.LogWarning(message: string, [<ParamArray>] parameters: obj[]) =
            if this.IsEnabled LogLevel.Warning then
                LogState.Create(LogLevel.Warning, message, parameters) |> this.Log

        member this.LogInformation(message: string, [<ParamArray>] parameters: obj[]) =
            if this.IsEnabled LogLevel.Information then
                LogState.Create(LogLevel.Information, message, parameters) |> this.Log
