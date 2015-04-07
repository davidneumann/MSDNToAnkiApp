module ExtractorHelpers

open HtmlAgilityPack
  
let htmlDecode str = System.Net.WebUtility.HtmlDecode(str)

let selectNodes (node:HtmlNode) (xpath:string) = node.SelectNodes(xpath)

let trySelectNodes node xpath = try Some(selectNodes node xpath) with | _ -> None

let extractTableInfo (headings:HtmlNodeCollection) targetHeading =
  if headings |> Seq.exists (fun h -> h.InnerText.Trim() = targetHeading) then
    headings |> Seq.find (fun h -> h.InnerText.Trim() = targetHeading) 
      |> (fun h -> selectNodes h "..//tr//td[1]//a") |> Seq.map (fun n -> n.Attributes.["href"].Value)
  else Seq.empty

let extractTableInfo2Column (headings:HtmlNodeCollection) targetHeading =
  if headings |> Seq.exists (fun h -> h.InnerText.Trim() = targetHeading) then
    let firstColumn = headings |> Seq.find (fun h -> h.InnerText.Trim() = targetHeading) 
                        |> (fun h -> selectNodes h "..//tr//td[1]") |> Seq.map (fun n -> n.InnerText.Trim())
    let secondColumn = headings |> Seq.find (fun h -> h.InnerText.Trim() = targetHeading) 
                        |> (fun h -> selectNodes h "..//tr//td[2]") |> Seq.map (fun n -> n.InnerText.Trim())
    Seq.zip firstColumn secondColumn
  else Seq.empty

let getHeadings url =
  let web = new HtmlWeb()
  let doc = web.Load(url)
  selectNodes doc.DocumentNode "//h2[@class='LW_CollapsibleArea_TitleDiv']"

