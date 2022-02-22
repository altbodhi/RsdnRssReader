module RsdnRss.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open RsdnRss.Core

type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/{id}">] ForumId of id: string

type Model =
    { page: Page
      Forums: Tag list
      currentForum: Forum option
      currentTalk: Talk option }


let initModel =
    { page = Home
      Forums = []
      currentForum = None
      currentTalk = None }

type RsdnService =
    { getForums: unit -> Async<Tag list>
      getForum: (string) -> Async<Forum>
      getTalk: (Item) -> Async<Talk> }

    interface IRemoteService with
        member this.BasePath = "/service"

type Message =
    | SetPage of Page
    | GetForums
    | GotForums of Tag list
    | GetForum of string
    | GotForum of Forum
    | GetTalk of Item
    | GotTalk of Talk
    | Error of exn
    | ClearTalk


let update remote message model =
    match message with
    | SetPage page ->
        match page with
        | Home -> { model with page = page }, Cmd.OfAsync.either remote.getForums () GotForums Error
        | ForumId id ->
            do printfn $"SetPage {page}"
            { model with page = page }, Cmd.ofMsg (GetForum id)
    | GotForum forum ->
        do printfn $"GotForum {forum.Title}"
        { model with currentForum = Some forum }, Cmd.none
    | GotForums ids -> { model with Forums = ids }, Cmd.none
    | GetForum id ->
        do printfn $"GetForum {id}"
        model, Cmd.OfAsync.either remote.getForum id GotForum Error
    | GetForums -> model, Cmd.OfAsync.either remote.getForums () GotForums Error
    | GetTalk i ->
        do printfn $"GetTalk {i.Title}"
        model, Cmd.OfAsync.either remote.getTalk i GotTalk Error
    | GotTalk t ->
        do printfn $"GotTalk {t.Question.Title}"
        { model with currentTalk = Some t }, Cmd.none
    | ClearTalk -> { model with currentTalk = None }, Cmd.none
    | Error exn ->
        do printfn $"{exn}"
        model, Cmd.none

let router =
    Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage model dispatch = Main.Home().Elt()

let talkView (model: Model) dispatch =
    Main
        .MessageList()
        .Class(
            if model.currentTalk.IsSome then
                "is-active"
            else
                ""
        )
        .Title(
            match model.currentTalk with
            | Some t -> t.Question.Title
            | None -> "Дискуссия неопределена"
        )
        .CloseTalk(fun e -> dispatch ClearTalk)
        .Rows(
            cond model.currentTalk
            <| function
                | None -> tr [] [ text "Пусто" ]
                | Some f ->
                    forEach f.Asnwers
                    <| fun it ->
                        tr [] [
                            td [] [ text (it.Author.Split().[0]) ]
                            td [] [
                                text (it.PubDate.ToString("dd\/MM HH:mm"))
                            ]
                            td [] [ RawHtml it.Detail ]
                        ]
        )
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main
        .MenuItem()
        .Active(
            match page with
            | Home ->
                if model.page = page then
                    "is-active"
                else
                    ""
            | ForumId fp ->
                match model.page with
                | Home ->
                    if model.page = page then
                        "is-active"
                    else
                        ""
                | ForumId fm -> if fp = fm then "is-active" else ""
        )
        .Url(router.Link page)
        .Text(text)
        .Elt()

let forumLinks (model: Model) =
    model.Forums
    |> List.map (fun t -> menuItem model (ForumId t.Id) t.Title)

let view model dispatch =
    Main()
        .Menu(
            concat (
                menuItem model Home "Update List"
                :: forumLinks model
            )
        )
        .Body(
            cond model.page
            <| function
                | Home -> homePage model dispatch
                | ForumId _ -> homePage model dispatch
        )
        .Questions(
            Main
                .QuestionList()
                .Title(
                    match model.currentForum with
                    | None -> "Выберите форум"
                    | Some f -> f.Title
                )
                .Rows(
                    cond model.currentForum
                    <| function
                        | None -> tr [] [ text "Пусто" ]
                        | Some f ->
                            forEach f.Items
                            <| fun it ->
                                tr [] [
                                    td [] [
                                        button [ attr.``class`` $"button is-outlined"
                                                 on.click (fun _ -> dispatch (GetTalk it)) ] [
                                            text it.Title
                                        ]
                                    ]
                                    td [] [ text (it.Author.Split().[0]) ]
                                    td [] [
                                        text (it.PubDate.ToString("dd\/MM HH:mm"))
                                    ]
                                    td [] [ RawHtml it.Detail ]
                                ]
                )
                .Messages(talkView model dispatch)
                .Elt()
        )

        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let rsdnService = this.Remote<RsdnService>()
        let update = update rsdnService

        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetForums) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
