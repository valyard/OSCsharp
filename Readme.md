## OSCsharp library

**OSC** library for **C#** and **Unity**.

This version has some small modifications to run under IOS without loosing the ability to use delegates in your c# code
Look at http://forum.unity3d.com/threads/113750-ExecutionEngineException-on-iOS-only 
You have to compile this project with VS 2008!

Based on [Bespoke Open Sound Control Library](http://www.bespokesoftware.org/wordpress/?page_id=69) by Paul Varcholik (pvarchol@bespokesoftware.org).  
Licensed under MIT license.

## Features
- Full OSC spec implementation,
- Supports Osc Messages and Bundles,
- Easy API,
- Can receive OSC messages over UDP,
- Unicast, broadcast, and multicast,
- Works in Unity and .NET 2.
- Works on IOS with delegates