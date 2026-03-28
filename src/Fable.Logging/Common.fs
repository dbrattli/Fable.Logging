namespace Fable.Logging

open System
open System.Collections.Generic
open System.Text.RegularExpressions

module Common =
    /// Pattern to match a log-format string with named placeholders
    let pattern = Regex(@"\{([a-zA-Z_]+\d|[a-zA-Z_]*?)\}")

    // Translate format string from named placeholders to string format with indexed placeholders
    let translateFormat (categoryName: string) (format: string) (args: obj array) =
        let parameters = Dictionary<string, obj>()
        let mutable index = -1

        let replacement (m: Match) =
            index <- index + 1
            parameters[m.Groups.[1].Value] <- args.[index]
            "{" + index.ToString() + "}"

        let format = pattern.Replace(format, replacement)

        let event =
            let format = String.Format(format, args = args)

            if categoryName.Length > 0 then
                categoryName + " - " + format
            else
                format

        parameters["event"] <- event
        parameters["category_name"] <- categoryName
        event, parameters
