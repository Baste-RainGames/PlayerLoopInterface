# PlayerLoopInterface
A simple interface for interacting with Unity's player loop system

## About

Unity exposes the PlayerLoop to allow you to insert your own "systems" to be run in similar ways to eg. Update or FixedUpate.
The interface for that is a bit hairy, and there are bugs that needs workarounds, so this is a nice wrapper interface for interacting with that system.

## Usage

Use PlayerLoopInterface.InsertSystemBefore/After to have a callback be executed every frame, before or after some built-in system.
The built-in systems live in UnityEngine.Experimental.PlayerLoop.

Here's an example that adds an entry point that will be called every frame, just before Update:

```cs
public static void MyCustomSystem {

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize() {
        PlayerLoopInterface.InsertSystemBefore(typeof(MyCustomSystem), UpdateSystem, typeof(UnityEngine.Experimental.PlayerLoop.Update);
    }
    
    private static void UpdateSystem() {
        Debug.Log("I get called once per frame!");
    }
}
```` 


## Installation

Modify your Packages/Manifest.json to include this package:

```
{
  "dependencies": {
    "com.baste.playerloopinterface": "https://github.com/Baste-RainGames/PlayerLoopInterface.git",`
    ...
```
