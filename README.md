# PlayerLoopInterface
A simple interface for interacting with Unity's player loop system

## 

Unity exposes the PlayerLoop to allow you to insert your own "systems" to be run in similar ways to eg. Update or FixedUpate.
The interface for that is a bit hairy, and there are bugs that needs workarounds, so this is a nice wrapper interface for interacting with that system.

In essence, use PlayerLoopInterface.InsertSystemBefore/After to have a callback be executed every frame, before or after some built-in system.
The built-in systems live in UnityEngine.Experimental.PlayerLoop, so if you want to insert a system to run just before Update, call:

PlayerLoopInterface.InsertSystemBefore(typeof(MyType), MyMethod, typeof(UnityEngine.Experimental.PlayerLoop.Update);

## Installation

Modify your Packages/Manifest.json to include this package:

`
{
  "dependencies": {
    "com.baste.playerloopinterface": "https://github.com/Baste-RainGames/PlayerLoopInterface.git",`
    ...
`
