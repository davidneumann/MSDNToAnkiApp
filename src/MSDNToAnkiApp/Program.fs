open System
open HtmlAgilityPack

let htmlDecode str =
  System.Net.WebUtility.HtmlDecode(str)

let selectNodes (node:HtmlNode) (xpath:string) =
  node.SelectNodes(xpath)

let trySelectNodes node xpath =
  try Some(selectNodes node xpath) with | _ -> None

let extractTableInfo (headings:HtmlNodeCollection) targetHeading =
  if headings |> Seq.exists (fun h -> h.InnerText.Trim() = targetHeading) then
    headings |> Seq.find (fun h -> h.InnerText.Trim() = targetHeading) 
      |> (fun h -> selectNodes h "..//tr//td[1]//a") |> Seq.map (fun n -> n.Attributes.["href"].Value)
  else Seq.empty

let extractTableInfo2Column (headings:HtmlNodeCollection) targetHeading =
  if headings |> Seq.exists (fun h -> h.InnerText.Trim() = targetHeading) then
    let firstColumn = headings |> Seq.find (fun h -> h.InnerText.Trim() = targetHeading) 
                        |> (fun h -> selectNodes h "..//tr//td[1]//a") |> Seq.map (fun n -> n.Attributes.["href"].Value)
    let secondColumn = headings |> Seq.find (fun h -> h.InnerText.Trim() = targetHeading) 
                        |> (fun h -> selectNodes h "..//tr//td[2]//a") |> Seq.map (fun n -> n.Attributes.["href"].Value)
    Seq.zip firstColumn secondColumn
  else Seq.empty

let getHeadings url =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  selectNodes doc.DocumentNode "//h2[@class='LW_CollapsibleArea_TitleDiv']"

let parseValue url =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  let name = htmlDecode (doc.DocumentNode.SelectSingleNode("//h1[@class='title']").InnerText.Trim())
  let description = htmlDecode (doc.DocumentNode.SelectSingleNode("//div[@class='introduction']/p[1]").InnerText.Trim())
  
  ()
  //todo: Extract the screenshot of any example code
  //todo: Export these to cards

let parseTypeAbbreviation url =
  failwith "Not implemented"

let parseType url =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  let name = htmlDecode (doc.DocumentNode.SelectSingleNode("//h1[@class='title']").InnerText.Trim())
  let description = htmlDecode (doc.DocumentNode.SelectSingleNode("//div[@class='introduction']/p[1]").InnerText.Trim())
  let headings = selectNodes doc.DocumentNode "//h2[@class='LW_CollapsibleArea_TitleDiv']"
  
  let instanceMembers = extractTableInfo2Column headings "Instance Members"
  let staticMembers = extractTableInfo2Column headings "Static Members"
  let unionCases = extractTableInfo2Column headings "Union Cases"
  
  ()
  //todo: Export these to cards

let parseModule url =
  let headings = getHeadings url
  extractTableInfo headings "Values" |> Seq.iter parseValue
  //todo: Extract screenshot of any example code (see https://msdn.microsoft.com/en-us/library/ee353880.aspx)

let parseNamespace url =
  let headings = getHeadings url
  
  extractTableInfo headings "Modules" |> Seq.iter parseModule
  extractTableInfo headings "Type Definitions" |> Seq.iter parseType
  extractTableInfo headings "Type Abbreviations" |> Seq.iter parseTypeAbbreviation

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
