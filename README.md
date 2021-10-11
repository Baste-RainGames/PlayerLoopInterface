# PlayerLoopInterface
A simple interface for interacting with Unity's player loop system

## About

Unity exposes the PlayerLoop to allow you to insert your own "systems" to be run in similar ways to eg. Update or FixedUpate.
The interface for that is a bit hairy, and there are bugs that needs workarounds, so this is a nice wrapper interface for interacting with that system.

## Installation

Modify your Packages/Manifest.json to include this package:

```
{
  "dependencies": {
    "com.baste.playerloopinterface": "https://github.com/Baste-RainGames/PlayerLoopInterface.git",
    ...
```

## Quick Use

Use PlayerLoopInterface.InsertSystemBefore/After to have a callback be executed every frame, before or after some built-in system.
The built-in systems can be found under UnityEngine.PlayerLoop.

Here's an example that adds an entry point that will be called every frame, just before Update:

```cs
public static class MyCustomSystem {

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize() {
        PlayerLoopInterface.InsertSystemBefore(typeof(MyCustomSystem), UpdateSystem, typeof(UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate));
    }

    private static void UpdateSystem() {
        Debug.Log("I get called once per frame!");
    }
}
```

If you want a function to run in the fixed timestep (FixedUpdate), you'll have to insert it as a subsystem of UnityEngine.PlayerLoop.FixedUpdate. 
A good alternative is to insert it before or after the UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate, which is the system that runs the FixedUpdate parts of MonoBehaviours:

```cs
PlayerLoopInterface.InsertSystemBefore(typeof(MyFixedTimestepSystem), MyFixedTimestepMethod, typeof(UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate));
```

## Details, Misc

Use `PlayerLoopInterface.CurrentLoopToString()` to get a string representation of the entire current player loop. This can be useful for understanding which systems exist, and for debugging your own systems.

You can also insert a full PlayerLoopSystem instead of using the (Type, delegate) helper methods provided. This gives you access to all of the features of the PlayerLoopSystem. Here they are, with some details on what they are and do (the official docs at https://docs.unity3d.com/ScriptReference/LowLevel.PlayerLoopSystem.html) are a bit scarce)

```cs
var mySystem = new PlayerLoopSystem {
    // The type seems to just be a marker. You can pass in pretty much whatever here.
    type = typeof(MyCustomSystem),

    // This is the C# method that gets called when the system runs. All of the builtin systems has a null delegate here.
    updateDelegate = MyMethod,

    /* This is a System.IntPtr. It's set for all of the built in leaf systems (see subSystemList). It's probably a pointer 
     * to the c++ engine function that's run for those systems. Copying one could, in theory, allow you to eg. run the 
     * builtin Update as your own thing, if you wanted? 
     */
    updateFunction = IntPtr.Zero,

    /* Red herring. Setting this doesn't seem to have any effect. For the builtin systems, only 
     * UnityEngine.PlayerLoop.FixedUpdate has this set to a value, but copying that value doesn't seem to do anything */
    loopConditionFunction = IntPtr.Zero,

    /* List of subsystems. Builtin systems uses a null value rather than an empty array for no subsystems.
     * Note that systems are structs, so managing the subsystem relations require a lot of care to 
     * 
     * Systems are organized in a tree-like structure. The builtin has a root system (with type Null), which has a few 
     * subsystems - like EarlyUpdate, FixedUpdate and Update. Those seem to be "folder" systems, as none of them have 
     * an updateFunction set. The children of these again are leaf systems with no subsystems, but an updateFunction.
     * I recommend running PlayerLoopInterface.CurrentLoopToString() to get an overview. */
    subSystemList = new PlayerLoopSystem[] { ... }
};

PlayerLoopInterface.InsertSystemBefore(mySystem, typeof(UnityEngine.PlayerLoop.Update));
```

## Known Issues

- When you make changes to the PlayerLoopSystem, they don't get changed back after exiting play mode. This causes systems you have added to be run at edit time (for some reason), which causes all kinds of problems. According to Unity, this is "by design". Because of this, the PlayerLoopInterface makes a backup of the player loop on startup, and resets to that default whenever you exit play mode. 

- Because of the above and the fact that PlayerLoopSystems are structs, I don't know of a way to support working together with other systems that edits the PlayerLoopSystem. This means that PlayerLoopInterface is incompatible with any other system that edits the PlayerLoopSystem!

- Relating to the above - Probably doesn't work with DOTS! I haven't tried, but I know that the PlayerLoopSystem is supposed to be used a bunch by DOTS. You probably won't need this in a DOTS-based world, since that's made to run your own systems in a different way.


