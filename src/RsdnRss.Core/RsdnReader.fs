namespace RsdnRss.Core

#if INTERACTIVE
#r "nuget: FSharp.Data"
#endif
open FSharp.Data
open System


type Item =
    { Title: string
      Detail: string
      Link: string
      PubDate: DateTimeOffset
      Author: string
      CommentRss: string }

type Forum = {Id : string; Title: string; Items: Item list }

type Asnwer =
    { Title: string
      Detail: string
      PubDate: DateTimeOffset
      Author: string }

type Talk =
    { Question: Item
      Title: string
      Asnwers: Asnwer list }

type Tag = {
  Id : string
  Title : string
}

type RsdnForumType = XmlProvider<"https://rsdn.org/forum/rss/cpp", Global=true>

type RsdnCommentType = XmlProvider<"https://rsdn.org/Forum/RSS/8179465", Global=true>

module RsdnReader =

    let getTags (): Async<Tag list> = 
        async {
          let! results = HtmlDocument.AsyncLoad ("https://rsdn.org/forum/list")

          let data =
            results.Descendants ["a"]
            |> Seq.choose (fun x ->
                  x.TryGetAttribute("href")
                  |> Option.map (fun a -> x.InnerText(), a.Value()))

          let tags = 
            data
            |> Seq.filter (fun (txt,target) -> target.StartsWith("/forum") &&  not (target.EndsWith("/list") || target.EndsWith("/mainlist")))
            |> Seq.map (fun (txt,target) -> { Id = target.Replace("/forum/",""); Title = txt})
            |> Seq.toList

          return tags
        }

    let getForum (id: string) : Async<Forum> =
        async {
            let uri = $"https://rsdn.org/forum/rss/{id}"

            let! data = RsdnForumType.AsyncLoad uri

            let forumData =
                data
                |> (fun e ->
                    { Id = id
                      Title = e.Channel.Title
                      Items =
                        e.Channel.Items
                        |> Seq.map (fun i ->
                            { Title = i.Title
                              Detail = i.Description
                              Link = i.Guid.Value
                              PubDate = i.PubDate
                              Author = i.Author
                              CommentRss = i.CommentRss })
                        |> Seq.toList })

            return forumData
        }



    let getTalk question =
      async {
        let! data = RsdnCommentType.AsyncLoad question.CommentRss
        let comments =
          data |> (fun e ->
            { Question = question
              Title = e.Channel.Title
              Asnwers =
                e.Channel.Items
                |> Seq.map (fun i ->
                    { Title = i.Title
                      Detail = i.Description
                      PubDate = i.PubDate
                      Author = i.Author })
                |> Seq.toList })

        return comments }

