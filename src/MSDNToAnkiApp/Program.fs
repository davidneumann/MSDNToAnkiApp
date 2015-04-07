open System
open HtmlAgilityPack

open ExtractorHelpers

type Card = {Front:string; Back:string; Image:string}

let parseValue url =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  let name = htmlDecode (doc.DocumentNode.SelectSingleNode("//h1[@class='title']").InnerText.Trim())
  let description = htmlDecode (doc.DocumentNode.SelectSingleNode("//div[@class='introduction']/p[1]").InnerText.Trim())
  
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
 
  let compactTable data = Seq.fold (fun acum (mem, desc) -> acum + "<b>"+mem+"</b>"+" -> "+desc+"<br>") "" data
  let instanceMembers = extractTableInfo2Column headings "Instance Members"  |> compactTable
  let staticMembers = extractTableInfo2Column headings "Static Members" |> compactTable
  let unionCases = extractTableInfo2Column headings "Union Cases" |> compactTable
  
  [{Front="Instance members of " + name; Back=instanceMembers; Image=""};
   {Front="Static members of " + name; Back=staticMembers; Image=""};
   {Front="Unions cases of " + name; Back=unionCases; Image=""}]

let rec parseModule url =
  let headings = getHeadings url
  extractTableInfo headings "Values" |> Seq.map parseValue
  extractTableInfo headings "Modules" |> Seq.iter parseModule
  //todo: Extract screenshot of any example code (see https://msdn.microsoft.com/en-us/library/ee353880.aspx)

let rec parseNamespace url =
  let headings = getHeadings url
  
  extractTableInfo headings "Modules" |> Seq.iter parseModule
  extractTableInfo headings "Type Definitions" |> Seq.map parseType
  extractTableInfo headings "Type Abbreviations" |> Seq.map parseTypeAbbreviation
  extractTableInfo headings "Namespaces" |> Seq.iter parseNamespace

let parseLibrary (url:string) =
  extractTableInfo (getHeadings url) "Related Topics" |> Seq.iter parseNamespace

[<EntryPoint>]
let main argv = 
  parseLibrary "https://msdn.microsoft.com/en-us/library/ee353567.aspx"
  0
