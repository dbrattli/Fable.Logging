module Fable.Logging.Tests.Utils

let equal (expected: 'T) (actual: 'T) =
    if expected <> actual then
        failwithf "Expected %A but got %A" expected actual

let notEqual (expected: 'T) (actual: 'T) =
    if expected = actual then
        failwithf "Expected values to differ but both were %A" expected

/// A mock logger provider that records all log calls for assertions.
type MockLogger(name: string) =
    let logs = ResizeArray<Fable.Logging.LogState>()

    member _.Logs = logs |> Seq.toList
    member _.Name = name

    member val MinimumLevel = Fable.Logging.LogLevel.Trace with get, set

    interface Fable.Logging.ILogger with
        member x.Log(state: Fable.Logging.LogState) =
            if state.Level >= x.MinimumLevel then
                logs.Add(state)

        member x.IsEnabled(logLevel: Fable.Logging.LogLevel) =
            logLevel >= x.MinimumLevel

        member _.BeginScope(_) : System.IDisposable = failwith "Not implemented"

type MockLoggerProvider() =
    let loggers = ResizeArray<MockLogger>()

    member _.Loggers = loggers |> Seq.toList

    interface Fable.Logging.ILoggerProvider with
        member _.CreateLogger(name) =
            let logger = MockLogger(name)
            loggers.Add(logger)
            logger

        member _.Dispose() = ()

type FactAttribute() =
    inherit System.Attribute()
