﻿module AwesomeApp

open Fable.Import
open Fable.Import.Fetch
open Fable.Helpers.Fetch
open Fable.Core.Extensions
open Elmish

// Model
type Status =
  | NotStarted
  | InProgress
  | Complete

type Model =
  { Url:string
    StatusStr:string
    Status:Status }

type Msg =
  | ChangeInput of string
  | SendRequest
  | Success of string
  | Error of obj


let init () =
  { Url = "https://httpbin.org/ip"
    StatusStr = "Request not yet sent"
    Status = NotStarted }, []

// Helpers
let getUrl ( url : string ) =
  promise {
    let! res = GlobalFetch.fetch url
    return! res.text()
  }

let statusString s =
  match s with
  | NotStarted -> "Request not yet started"
  | InProgress -> "Request in progress"
  | Complete -> "Request finished"

// Update
let update msg model : Model*Cmd<Msg> =
  match msg with
  | ChangeInput str ->
    { model with Url = str}, []
  | SendRequest ->
    { model with
        StatusStr = (statusString InProgress)
        Status = InProgress }, Cmd.ofPromise getUrl model.Url Success Error
  | Success str->
    { model with
        StatusStr = str
        Status = Complete }, []
  | Error e ->
    { model with
        StatusStr = string e
        Status = Complete }, []

module R = Fable.Helpers.ReactNative
open R.Props

let view model dispatch =
  R.view [] [
    R.textInput
      [ TextInputProperties.Style
          [ Height 20.
            JustifyContent JustifyContent.SpaceAround]
        OnChangeText (ChangeInput >> dispatch)
        AutoFocus true
        Placeholder "http://ip.jsontest.com/"]
      ""
    Styles.button "Send Request" (fun _ -> dispatch SendRequest)
    R.text [] (string model.StatusStr)
    R.text [] (statusString model.Status)
  ]

// App
let program =
    Program.mkProgram init update
    |> Program.withConsoleTrace

type App() as this =
    inherit React.Component<obj, Model>()

    let safeState state =
        match unbox this.props with 
        | false -> this.state <- state
        | _ -> this.setState state

    let dispatch = program |> Program.run safeState

    member this.componentDidMount() =
        this.props <- true

    member this.render() =
        view this.state dispatch
