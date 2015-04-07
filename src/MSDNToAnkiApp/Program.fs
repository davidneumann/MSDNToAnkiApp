open System
open HtmlAgilityPack

let selectNodes (node:HtmlNode) (xpath:string) =
  node.SelectNodes(xpath)

let trySelectNodes node xpath =
  try Some(selectNodes node xpath) with | _ -> None

let parseTypeAbbreviation url =
  failwith "Not implemented"

let parseType url =
  failwith "Not implemented"

let parseModule url =
  failwith "Not implemented"

let parseNamespace url =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  let headings = selectNodes doc.DocumentNode "//h2[@class='LW_CollapsibleArea_TitleDiv']"
  headings |> Seq.iter (fun h -> printfn "%A" (h.InnerText.Trim()))
  
  let modules = 
    if headings |> Seq.exists (fun h -> h.InnerText.Trim() = "Modules") then
      headings |> Seq.find (fun h -> h.InnerText.Trim() = "Modules") |> (fun h -> selectNodes h "..//tr//td[1]//a") |> Seq.map (fun n -> n.Attributes.["href"].Value)
    else Seq.empty
  let types = 
    if headings |> Seq.exists (fun h -> h.InnerText.Trim() = "Type Definitions") then
      headings |> Seq.find (fun h -> h.InnerText.Trim() = "Type Definitions") |> (fun h -> selectNodes h "..//tr//td[1]//a") |> Seq.map (fun n -> n.Attributes.["href"].Value)
    else Seq.empty
  let typeAbbrevations =
    if headings |> Seq.exists (fun h -> h.InnerText.Trim() = "Type Abbreviations") then
      headings |> Seq.find (fun h -> h.InnerText.Trim() = "Type Abbreviations") |> (fun h -> selectNodes h "..//tr//td[1]//a") |> Seq.map (fun n -> n.Attributes.["href"].Value)
    else Seq.empty
  
  modules |> Seq.iter parseModule
  types |> Seq.iter parseType
  typeAbbrevations |> Seq.iter parseTypeAbbreviation

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
