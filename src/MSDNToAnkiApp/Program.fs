open System
open HtmlAgilityPack

let parseNamespace url =
  let web = new HtmlWeb()
  let doc = web.Load(url)

  failwith "Not implemented"

let parseModule url =
  failwith "Not implemented"

let parseType url =
  failwith "Not implemented"

let parseTypeAbbreviation url =
  failwith "Not implemented"

let parseLibrary (url:string) =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  let namespaces = doc.DocumentNode.SelectNodes("//a[@id='sectionToggle1']/../div//tr//td[1]//a")
                    |> Seq.map (fun a -> a.Attributes.["href"].Value)
  namespaces |> Seq.iter parseNamespace

[<EntryPoint>]
let main argv = 
  parseLibrary "https://msdn.microsoft.com/en-us/library/ee353567.aspx"
  0
