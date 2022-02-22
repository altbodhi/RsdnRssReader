namespace RsdnRss.Server

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Hosting
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Server
open RsdnRss
open RsdnRss.Core

type RsdnService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<RsdnRss.Client.Main.RsdnService>()

    let loadForumList () =
        let json =
            Path.Combine(env.ContentRootPath, "data/forums.json")
            |> File.ReadAllText

        JsonSerializer.Deserialize<string list>(json)

    override this.Handler =
        { getForums = fun () -> async { 
            let! tags = RsdnReader.getTags ()
            return tags 
            }

          getForum =
              fun (id) ->
                  async {
                      do printfn $"Client request [Forum with Id = {id}]"
                      let! forum = RsdnReader.getForum id
                      return forum
                  }

          getTalk =
              fun (item) ->
                  async {
                      do printfn $"Client request [Talk for item = {item}]"
                      let! talk = RsdnReader.getTalk item
                      return talk
                  } }
