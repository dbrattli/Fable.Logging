module Fable.Logging.Tests.Common

open Fable.Logging
open Fable.Logging.Tests.Utils

[<Fact>]
let ``test translateFormat with no args`` () =
    let event, parameters = Common.translateFormat "" "Hello world" [||]
    event |> equal "Hello world"
    parameters["event"] |> equal (box "Hello world")

[<Fact>]
let ``test translateFormat with named placeholder`` () =
    let event, parameters = Common.translateFormat "" "Hello {name}" [| box "World" |]
    event |> equal "Hello World"
    parameters["name"] |> equal (box "World")

[<Fact>]
let ``test translateFormat with multiple placeholders`` () =
    let event, parameters = Common.translateFormat "" "{action} {count} items" [| box "Processed"; box 42 |]
    event |> equal "Processed 42 items"
    parameters["action"] |> equal (box "Processed")
    parameters["count"] |> equal (box 42)

[<Fact>]
let ``test translateFormat with category name`` () =
    let event, _ = Common.translateFormat "MyApp.Service" "Hello world" [||]
    event |> equal "MyApp.Service - Hello world"

[<Fact>]
let ``test translateFormat empty category name`` () =
    let event, _ = Common.translateFormat "" "Hello world" [||]
    event |> equal "Hello world"

[<Fact>]
let ``test translateFormat sets category_name parameter`` () =
    let _, parameters = Common.translateFormat "TestCategory" "msg" [||]
    parameters["category_name"] |> equal (box "TestCategory")
