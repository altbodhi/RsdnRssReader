module RsdnRss.Server.Index

open Bolero
open Bolero.Html
open Bolero.Server.Html
open RsdnRss
open System

let page =
    doctypeHtml {
        head {
            meta {
                attr.name "viewport"
                attr.content "width=device-width, initial-scale=1.0"
            }

            meta { attr.charset "UTF-8" }
            title { text "View RSDN Forums" }
            ``base`` { attr.href "/" }

            link {
                attr.rel "stylesheet"
                attr.href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.4/css/bulma.min.css"
            }

            link {
                attr.rel "stylesheet"
                attr.href "css/index.css"
            }
        }


        body {
            nav {
                attr.``class`` "navbar; is-dark"
                attr.name "role"
                attr.value "navigation"
                attr.aria "label" "main navigation"
            }

            div {
                attr.``class`` "navbar-brand"

                a {
                    attr.``class`` "navbar-item;has-text-weight-bold;is-size-5"
                    attr.href "/"
                }

                img { 
                    attr.style "height:40px" 
                    attr.src "https://rsdn.org/Content/logo.svg" 
                    text "" }
            }


            div {
                attr.id "main"
                comp<Client.Main.MyApp>
            }

            boleroScript
        }
    }

(*
let page =
    doctypeHtml {
        head {
            meta { attr.charset "UTF-8" }

            meta {
                attr.name "viewport"
                attr.content "width=device-width, initial-scale=1.0"
            }

            title { text "View RSDN Forums" }
            ``base`` { attr.href "/" }

            link {
                attr.rel "stylesheet"
                attr.href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.4/css/bulma.min.css"
            }

            link {
                attr.rel "stylesheet"
                attr.href "css/index.css"
            }
        }

        body {
            nav
                { attr.``class`` "navbar; is-dark" "role"
                  => "navigation" attr.aria "label" "main navigation" }
                div
                { attr.``class``
                    "navbar-brand"
                    a
                    { attr.``class``
                        (String.Join(";", [ "navbar-item"; "has-text-weight-bold"; "is-size-5" ]))
                        attr.href
                        "/" }
                    img
                    { attr.style "height:40px" attr.src "https://rsdn.org/Content/logo.svg" }
                    text
                    "" }


                div
                { attr.id "main" rootComp<Client.Main.MyApp> }
                boleroScript
        }
    }
*)
