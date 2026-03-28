module Fable.Logging.Tests.LoggerFactory

open Fable.Logging
open Fable.Logging.Tests.Utils

[<Fact>]
let ``test LoggerFactory.Create returns factory`` () =
    let factory = LoggerFactory.Create()
    let logger = factory.CreateLogger("test")
    // Logger exists but no providers, so IsEnabled is false
    logger.IsEnabled(LogLevel.Information) |> equal false

[<Fact>]
let ``test LoggerFactory with provider`` () =
    let provider = MockLoggerProvider()
    let factory = LoggerFactory.Create(fun builder -> builder.AddProvider(provider))
    let logger = factory.CreateLogger("MyApp")
    logger.IsEnabled(LogLevel.Information) |> equal true

[<Fact>]
let ``test LoggerFactory creates logger with correct name`` () =
    let provider = MockLoggerProvider()
    let factory = LoggerFactory.Create(fun builder -> builder.AddProvider(provider))
    let _logger = factory.CreateLogger("MyApp.Service")
    provider.Loggers.Length |> equal 1
    provider.Loggers.[0].Name |> equal "MyApp.Service"

[<Fact>]
let ``test LoggerFactory dispatches to provider`` () =
    let provider = MockLoggerProvider()
    let factory = LoggerFactory.Create(fun builder -> builder.AddProvider(provider))
    let logger = factory.CreateLogger("test")
    logger.Log(LogLevel.Information, "hello")
    provider.Loggers.[0].Logs.Length |> equal 1

[<Fact>]
let ``test LoggerFactory dispatches to multiple providers`` () =
    let provider1 = MockLoggerProvider()
    let provider2 = MockLoggerProvider()

    let factory =
        LoggerFactory.Create(fun builder ->
            builder.AddProvider(provider1)
            builder.AddProvider(provider2))

    let logger = factory.CreateLogger("test")
    logger.Log(LogLevel.Information, "hello")
    provider1.Loggers.[0].Logs.Length |> equal 1
    provider2.Loggers.[0].Logs.Length |> equal 1

[<Fact>]
let ``test LoggerFactory with configure sets minimum level`` () =
    let provider = MockLoggerProvider()

    let factory =
        LoggerFactory.Create(fun builder ->
            builder.AddProvider(provider)
            builder.SetMinimumLevel(LogLevel.Warning))

    let logger = factory.CreateLogger("test")
    logger.Log(LogLevel.Debug, "should be filtered")
    logger.Log(LogLevel.Warning, "should pass")
    provider.Loggers.[0].Logs.Length |> equal 1
    provider.Loggers.[0].Logs.[0].Level |> equal LogLevel.Warning

[<Fact>]
let ``test LoggerFactory.Create empty constructor`` () =
    use factory = LoggerFactory() :> ILoggerFactory
    let logger = factory.CreateLogger("test")
    // No providers => not enabled
    logger.IsEnabled(LogLevel.Information) |> equal false

[<Fact>]
let ``test LoggerFactory AddProvider after CreateLogger`` () =
    let provider = MockLoggerProvider()
    let factory = LoggerFactory.Create()
    let logger = factory.CreateLogger("test")
    // Initially no providers
    logger.IsEnabled(LogLevel.Information) |> equal false
    // Add provider after logger was created
    factory.AddProvider(provider)
    logger.IsEnabled(LogLevel.Information) |> equal true
    logger.Log(LogLevel.Information, "hello")
    provider.Loggers.[0].Logs.Length |> equal 1

[<Fact>]
let ``test LoggerFactory ClearProviders`` () =
    let provider = MockLoggerProvider()

    let factory =
        LoggerFactory.Create(fun builder ->
            builder.AddProvider(provider)
            builder.ClearProviders())

    let logger = factory.CreateLogger("test")
    logger.IsEnabled(LogLevel.Information) |> equal false

[<Fact>]
let ``test LoggerFactory Dispose clears providers`` () =
    let provider = MockLoggerProvider()
    let factory = LoggerFactory.Create(fun builder -> builder.AddProvider(provider))
    let _logger = factory.CreateLogger("test")
    (factory :> System.IDisposable).Dispose()
    // After dispose, creating a new logger should have no providers
    let logger2 = factory.CreateLogger("test2")
    logger2.IsEnabled(LogLevel.Information) |> equal false
