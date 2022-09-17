namespace Fable.Logging


type Logger(name: string, providers: ILoggerProvider seq) =
    let loggers = providers |> Seq.map (fun p -> p.CreateLogger name) |> Seq.toList

    member _.Log(state: LogState) =
        loggers |> List.iter (fun l -> l.Log state)

    member _.IsEnabled(logLevel: LogLevel) =
        loggers |> List.exists (fun l -> l.IsEnabled logLevel)

    static member Create(name: string, providers: ILoggerProvider seq) = Logger(name, providers)

    interface ILogger with
        member _.Log(state: LogState) =
            loggers |> List.iter (fun l -> l.Log state)

        member x.IsEnabled(logLevel: LogLevel) = x.IsEnabled logLevel

        member _.BeginScope(state: obj) = failwith "Not implemented"

type LoggerFactory(providers: ILoggerProvider seq) =
    let providers = ResizeArray<ILoggerProvider>(providers)

    interface ILoggerFactory with
        member this.CreateLogger name =
            let logger = Logger(name, providers)
            logger :> ILogger

        member this.AddProvider(provider) = providers.Add provider

        member this.Dispose() =
            let providers' = providers
            providers.Clear()

            for provider in providers' do
                provider.Dispose()


[<AutoOpen>]
module LoggerFactoryExtensions =
    type ILoggerFactory with

        member inline this.CreateLogger<'T>() =
            let name = nameof<'T>
            this.CreateLogger name
