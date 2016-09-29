﻿#if INTERACTIVE
#r "PresentationCore.dll"
#r "PresentationFramework.dll"
#r "System.Xaml.dll"
#r "UIAutomationTypes.dll"
#r "WindowsBase.dll"
#endif

open System
open System.IO
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media
open System.Windows.Media.Imaging

/// Bird type
type Bird = { X:float; Y:float; VY:float; IsAlive:bool }
/// Respond to flap command
let flap (bird:Bird) = { bird with VY = - System.Math.PI }
/// Applies gravity to bird
let gravity (bird:Bird) = { bird with VY = bird.VY + 0.1 }
/// Applies physics to bird
let physics (bird:Bird) = { bird with Y = bird.Y + bird.VY }
/// Updates bird with gravity & physics
let update = gravity >> physics
 
/// Generates the level's tube positions
let generateLevel n =
   let rand = System.Random()
   [for i in 1..n -> 50+(i*150), 32+rand.Next(160)]

let level = generateLevel 10

/// Converts specified bitmap to an image
let toImage (bitmap:#BitmapSource) =
   let w, h = float bitmap.PixelWidth, float bitmap.PixelHeight  
   Image(Source=bitmap,Stretch=Stretch.Fill,Width=w,Height=h) 
/// Loads image from file if it exists or the url otherwise
let load file url =
   let path = Path.Combine(__SOURCE_DIRECTORY__, file)
   let uri = 
      if File.Exists(path) 
      then Uri(path, UriKind.Relative)
      else Uri(url, UriKind.Absolute)
   BitmapImage(uri)

let bg = 
   load "bg.png" "http://flappycreator.com/default/bg.png"
   |> toImage
let ground = 
   load "ground.png" "http://flappycreator.com/default/ground.png"
   |> toImage
let tube1 = load "tube1.png" "http://flappycreator.com/default/tube1.png"
let tube2 = load "tube2.png" "http://flappycreator.com/default/tube2.png"
let bird_sing = 
   load "bird_sing.png" "http://flappycreator.com/default/bird_sing.png"
   |> toImage

let canvas = Canvas()
let move image (x,y) =
   Canvas.SetLeft(image, x)
   Canvas.SetTop(image, y)
let add image (x,y) = 
   canvas.Children.Add(image) |> ignore
   move image (float x, float y)

add bg (0,0)
add bird_sing (30,150)
// Level's tubes
let tubes =
   [for (x,y) in level ->
      let tube1 = toImage tube1
      let tube2 = toImage tube2
      add tube1 (x,y-320)
      add tube2 (x,y+100)
      (x,y), tube1, tube2]
add ground (0,360)

let scroll = ref 0
let flappy = ref { X = 30.0; Y = 150.0; VY = 0.0; IsAlive=true }
let flapme () = if (!flappy).IsAlive then flappy := flap !flappy

let window = Window(Title="Flap me",Width=288.0,Height=440.0)
window.Content <- canvas
window.MouseDown.Add(fun _ -> flapme())
window.KeyDown.Add(fun args -> if args.Key = Key.Space then flapme())
window.Show()

// Update scene
CompositionTarget.Rendering.Add(fun _ ->
   flappy := update !flappy
   let bird = !flappy
   move bird_sing (bird.X, bird.Y)
   for ((x,y),tube1,tube2) in tubes do
      move tube1 (float (x + !scroll),float (y-320))
      move tube2 (float (x + !scroll),float (y+100))
   decr scroll
)                                                                
                                                                