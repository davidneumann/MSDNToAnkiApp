open System
open HtmlAgilityPack

open ExtractorHelpers

type Card = {Front:string; Back:string; Image:string}

let printCard card =
  printfn "Front: %s\nBack: %s" card.Front card.Back

let parseValue url =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  let name = (htmlDecode (doc.DocumentNode.SelectSingleNode("//h1[@class='title']").InnerText.Trim())).Replace(" (F#)", "")
  let signature = 
    let raw = htmlDecode (doc.DocumentNode.SelectSingleNode("//div[@id='syntaxSection']//pre").InnerText.Trim().Replace("// Signature:", ""))
    let startSig = raw.Substring(raw.IndexOf(":") + 2)
    startSig.Remove(startSig.IndexOf(System.Environment.NewLine))
  let description = htmlDecode (doc.DocumentNode.SelectSingleNode("//div[@class='introduction']/p[1]").InnerText.Trim()) + "<br>" + signature
  
  {Front=name; Back=description; Image=""}
  //todo: Extract the screenshot of any example code
  //todo: Export these to cards

let parseTypeAbbreviation url =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  let name = htmlDecode (doc.DocumentNode.SelectSingleNode("//h1[@class='title']").InnerText.Trim())
  let description = htmlDecode (doc.DocumentNode.SelectSingleNode("//div[@class='introduction']/p[1]").InnerText.Trim())
  
  {Front=name; Back=description; Image=""}
  //todo: Extract the screenshot above the remark.
  //todo: export type abbrevations to cards.

let parseType url =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  let name = htmlDecode (doc.DocumentNode.SelectSingleNode("//h1[@class='title']").InnerText.Trim())
  let description = htmlDecode (doc.DocumentNode.SelectSingleNode("//div[@class='introduction']/p[1]").InnerText.Trim())
  let headings = selectNodes doc.DocumentNode "//h2[@class='LW_CollapsibleArea_TitleDiv']"
  
  try
    let compactTable data = Seq.fold (fun acum (mem, desc) -> acum + "<b>"+mem+"</b>"+" -> "+desc+"<br>") "" data
    let instanceMembers = extractTableInfo2Column headings "Instance Members"  |> compactTable
    let staticMembers = extractTableInfo2Column headings "Static Members" |> compactTable
    let unionCases = extractTableInfo2Column headings "Union Cases" |> compactTable
    
    [{Front=name; Back=description; Image=""};
     {Front="Instance members of " + name; Back=instanceMembers; Image=""};
     {Front="Static members of " + name; Back=staticMembers; Image=""};
     {Front="Unions cases of " + name; Back=unionCases; Image=""}] |> Seq.ofList
  with
    | _ -> printfn "Error on: %s" url; Seq.empty

let rec parseModule url =
  let headings = getHeadings url
  let values = extractTableInfo headings "Values" |> Seq.map parseValue
  Seq.append values (extractTableInfo headings "Modules" |> Seq.map parseModule |> Seq.concat)
  //todo: Extract screenshot of any example code (see https://msdn.microsoft.com/en-us/library/ee353880.aspx)

let rec parseNamespace url =
  let headings = getHeadings url
  
  let modules = extractTableInfo headings "Modules" |> Seq.map parseModule |> Seq.concat
  let types = extractTableInfo headings "Type Definitions" |> Seq.map parseType |> Seq.concat
  let typeAbrevations = extractTableInfo headings "Type Abbreviations" |> Seq.map parseTypeAbbreviation
  let namespaces = 
    try
      if url <> "https://msdn.microsoft.com/en-us/library/hh323978.aspx" then
        (extractTableInfo headings "Namespaces" |> Seq.map parseNamespace |> Seq.concat)
      else parseNamespace "https://msdn.microsoft.com/en-us/library/hh698410.aspx"
    with
      | _ as e -> printfn "Broken table on: %s\n%s" url (e.InnerException.ToString()); Seq.empty
  [modules;types;typeAbrevations;namespaces] |> Seq.ofList |> Seq.concat

let parseLibrary (url:string) =
  extractTableInfo (getHeadings url) "Related Topics" |> Seq.map parseNamespace

[<EntryPoint>]
let main argv = 
  let cards = parseLibrary "https://msdn.microsoft.com/en-us/library/ee353567.aspx" |> Seq.concat
  //todo: Save cards
  printfn "Card count: %d" (Seq.length cards)
  cards |> Seq.iter (fun c -> printfn "Front: %s" c.Front)
  0
