/////////////////////////////////////////////////////////
//     http-сервер на F#.     Автор: A.Б.Корниенко     //
/////////////////////////////////////////////////////////

namespace Global

open System
open System.IO
open System.Web
open System.Net
open System.Net.Sockets
open System.Diagnostics
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open System.Reflection
open System.Text

[<AutoOpen>]
module Public =

module Main =
