namespace Fable.Logging

open Fable.Core

type internal Logger(name: string, providers: ResizeArray<ILoggerProvider>, minimumLevel: LogLevel) =
    let loggers =
        providers
        |> Seq.map (fun p -> p.CreateLogger(name))
        |> ResizeArray

    member x.AddProviders(provider: ILoggerProvider) =
        loggers.Add(provider.CreateLogger(name))

    interface ILogger with
        member _.Log(state: LogState) =
            if state.Level >= minimumLevel then
                loggers
                |> Seq.iter (fun l -> l.Log state)

        member x.IsEnabled(logLevel: LogLevel) =
            loggers
            |> Seq.exists (fun l -> l.IsEnabled logLevel)

        member _.BeginScope(state: obj) = failwith "Not implemented"

[<Mangle>]
type ILoggingBuilder =
    abstract member AddProvider: ILoggerProvider -> unit
    abstract member ClearProviders: unit -> unit
    abstract member SetMinimumLevel: LogLevel -> unit

type LoggerFactory(providers: ILoggerProvider seq) =
    let providers = ResizeArray<ILoggerProvider>(providers)
    let mutable minimumLevel = LogLevel.Information
    let loggers = ResizeArray<Logger>()

    new() = new LoggerFactory(Seq.empty)

    interface ILoggerFactory with
        member this.CreateLogger(name) =
            let logger = Logger(name, providers, minimumLevel)
            loggers.Add(logger)
            logger :> ILogger

        member this.AddProvider(provider) =
            for logger in loggers do
                logger.AddProviders(provider)

            providers.Add(provider)

        member this.Dispose() =
            for provider in providers do
                provider.Dispose()

            providers.Clear()

    interface ILoggingBuilder with
        member x.AddProvider(provider: ILoggerProvider) = providers.Add(provider)

        member x.ClearProviders() =
            for provider in providers do
                provider.Dispose()

            providers.Clear()

        member x.SetMinimumLevel(logLevel: LogLevel) = minimumLevel <- logLevel

    member x.CreateLogger name =
        (x :> ILoggerFactory).CreateLogger(name)

    member x.AddProvider provider =
        (x :> ILoggerFactory).AddProvider(provider)

    static member Create() = new LoggerFactory()

    static member Create(configure: ILoggingBuilder -> unit) : LoggerFactory =
        let factory = new LoggerFactory()
        configure (factory :> ILoggingBuilder)
        factory
