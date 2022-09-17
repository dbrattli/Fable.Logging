namespace Fable.Logging

open System
open System.Collections.Generic
open System.Text.RegularExpressions

module Common =
    /// Pattern to match a log-format string with named placeholders
    let pattern = Regex(@"\{([a-zA-Z_]+\d|[a-zA-Z_]*?)\}")

    // Translate format string from named placeholders to string format with indexed placeholders
    let translateFormat (format: string) (args: obj array) =
        let parameters = Dictionary<string, obj>()
        let mutable index = -1

        let replacement (m: Match) =
            index <- index + 1
            parameters[m.Groups.[1].Value] <- args.[index]
            "{" + index.ToString() + "}"

        let format = pattern.Replace(format, replacement)
        format, parameters
