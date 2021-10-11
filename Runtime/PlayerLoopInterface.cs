using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_2020_1_OR_NEWER
using UnityEngine.LowLevel;
#else
using UnityEngine.Experimental.LowLevel;
using UnityEngine.Experimental.PlayerLoop;
#endif

/// <summary>
/// Unity exposes the PlayerLoop to allow you to insert your own "systems" to be run in similar ways to eg. Update or FixedUpate.
/// The interface for that is a bit hairy, and there are bugs that needs workarounds, so this is a nice interface for interacting with that system.
///
/// In essence, use PlayerLoopInterface.InsertSystemBefore/After to have a callback be executed every frame, before or after some built-in system.
/// The built-in systems live in UnityEngine.Experimental.PlayerLoop, so if you want to insert a system to run just before Update, call:
///
/// PlayerLoopInterface.InsertSystemBefore(typeof(MyType), MyMethod, typeof(UnityEngine.PlayerLoop.Update);
///
/// If you want to run in the fixed timestep (FixedUpdate), you have to insert the system as a subsystem of UnityEngine.PlayerLoop.FixedUpdate. For example, use
/// UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate:
///
/// PlayerLoopInterface.InsertSystemBefore(typeof(MyType), MyMethod, typeof(UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate);
/// </summary>
public static class PlayerLoopInterface {

    private static bool hasFetchedSystem;

    // We write to this
    private static PlayerLoopSystem rootSystem;

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize() {
        EnsureSystemFetched();
    }

    private static void EnsureSystemFetched() {
        if (hasFetchedSystem)
            return;

        var defaultSystem = PlayerLoop.GetDefaultPlayerLoop();
        rootSystem = CopySystem(defaultSystem);
        hasFetchedSystem = true;

        // if the Entities package is not installed, any systems registered keeps running after we leave play mode.
        // This is "intended behaviour". Not joking. https://fogbugz.unity3d.com/default.asp?1089518_lub560iemcggi1c9
        PlayerLoopQuitChecker.GameQuitCallback += () => {
            PlayerLoop.SetPlayerLoop(defaultSystem);
        };
    }

    private enum InsertType {
        Before,
        After
    }

    /// <summary>
    /// Inserts a new player loop system in the player loop, just after another system.
    /// </summary>
    /// <param name="newSystemMarker">Type marker for the new system.</param>
    /// <param name="newSystemUpdate">Callback that will be called each frame after insertAfter.</param>
    /// <param name="insertAfter">The subsystem to insert the system after.</param>
    public static void InsertSystemAfter(Type newSystemMarker, PlayerLoopSystem.UpdateFunction newSystemUpdate, Type insertAfter) {
        var playerLoopSystem = new PlayerLoopSystem {type = newSystemMarker, updateDelegate = newSystemUpdate};
        InsertSystemAfter(playerLoopSystem, insertAfter);
    }

    /// <summary>
    /// Inserts a new player loop system in the player loop, just before another system.
    /// </summary>
    /// <param name="newSystemMarker">Type marker for the new system.</param>
    /// <param name="newSystemUpdate">Callback that will be called each frame before insertBefore.</param>
    /// <param name="insertBefore">The subsystem to insert the system before.</param>
    public static void InsertSystemBefore(Type newSystemMarker, PlayerLoopSystem.UpdateFunction newSystemUpdate, Type insertBefore) {
        var playerLoopSystem = new PlayerLoopSystem {type = newSystemMarker, updateDelegate = newSystemUpdate};
        InsertSystemBefore(playerLoopSystem, insertBefore);
    }

    /// <summary>
    /// Inserts a new player loop system in the player loop, just after another system.
    /// </summary>
    /// <param name="toInsert">System to insert. Needs to have updateDelegate and Type set.</param>
    /// <param name="insertAfter">The subsystem to insert the system after</param>
    public static void InsertSystemAfter(PlayerLoopSystem toInsert, Type insertAfter) {
        if (toInsert.type == null)
            throw new ArgumentException("The inserted player loop system must have a marker type!", nameof(toInsert.type));
        if (toInsert.updateDelegate == null)
            throw new ArgumentException("The inserted player loop system must have an update delegate!", nameof(toInsert.updateDelegate));
        if (insertAfter == null)
            throw new ArgumentNullException(nameof(insertAfter));

        EnsureSystemFetched();

        InsertSystem(ref rootSystem, toInsert, insertAfter, InsertType.After, out var couldInsert);
        if (!couldInsert) {
            throw new ArgumentException($"When trying to insert the type {toInsert.type.Name} into the player loop after {insertAfter.Name}, " +
                                        $"{insertAfter.Name} could not be found in the current player loop!");
        }

        PlayerLoop.SetPlayerLoop(rootSystem);
    }

    /// <summary>
    /// Inserts a new player loop system in the player loop, just before another system.
    /// </summary>
    /// <param name="toInsert">System to insert. Needs to have updateDelegate and Type set.</param>
    /// <param name="insertBefore">The subsystem to insert the system before</param>
    public static void InsertSystemBefore(PlayerLoopSystem toInsert, Type insertBefore) {
        if (toInsert.type == null)
            throw new ArgumentException("The inserted player loop system must have a marker type!", nameof(toInsert.type));
        if (toInsert.updateDelegate == null)
            throw new ArgumentException("The inserted player loop system must have an update delegate!", nameof(toInsert.updateDelegate));
        if (insertBefore == null)
            throw new ArgumentNullException(nameof(insertBefore));

        EnsureSystemFetched();

        InsertSystem(ref rootSystem, toInsert, insertBefore, InsertType.Before, out var couldInsert);
        if (!couldInsert) {
            throw new ArgumentException($"When trying to insert the type {toInsert.type.Name} into the player loop before {insertBefore.Name}, " +
                                        $"{insertBefore.Name} could not be found in the current player loop!");
        }

        PlayerLoop.SetPlayerLoop(rootSystem);
    }

    /// <summary>
    /// Utility to get a string representation of the current player loop.
    /// Note that this is the current player loop as the PlayerLoopInterface believes it to be, if a different system changes the underlying player loop system,
    /// there's no way to get any info about that.
    /// </summary>
    /// <returns>String representation of the current player loop system.</returns>
    public static string CurrentLoopToString()
    {
        return PrintSystemToString(rootSystem);
    }

    public static PlayerLoopSystem CopySystem(PlayerLoopSystem system) {
        // PlayerLoopSystem is a struct.
        var copy = system;

        // but the sub system list is an array.
        if (system.subSystemList != null) {
            copy.subSystemList = new PlayerLoopSystem[system.subSystemList.Length];
            for (int i = 0; i < copy.subSystemList.Length; i++) {
                copy.subSystemList[i] = CopySystem(system.subSystemList[i]);
            }
        }

        return copy;
    }

    private static void InsertSystem(ref PlayerLoopSystem currentLoopRecursive, PlayerLoopSystem toInsert, Type insertTarget, InsertType insertType,
                                     out bool couldInsert) {
        var currentSubSystems = currentLoopRecursive.subSystemList;
        if (currentSubSystems == null) {
            couldInsert = false;
            return;
        }

        int indexOfTarget = -1;
        for (int i = 0; i < currentSubSystems.Length; i++) {
            if (currentSubSystems[i].type == insertTarget) {
                indexOfTarget = i;
                break;
            }
        }

        if (indexOfTarget != -1) {
            var newSubSystems = new PlayerLoopSystem[currentSubSystems.Length + 1];

            var insertIndex = insertType == InsertType.Before ? indexOfTarget : indexOfTarget + 1;

            for (int i = 0; i < newSubSystems.Length; i++) {
                if (i < insertIndex)
                    newSubSystems[i] = currentSubSystems[i];
                else if (i == insertIndex) {
                    newSubSystems[i] = toInsert;
                }
                else {
                    newSubSystems[i] = currentSubSystems[i - 1];
                }
            }

            couldInsert = true;
            currentLoopRecursive.subSystemList = newSubSystems;
        }
        else {
            for (var i = 0; i < currentSubSystems.Length; i++) {
                var subSystem = currentSubSystems[i];
                InsertSystem(ref subSystem, toInsert, insertTarget, insertType, out var couldInsertInInner);
                if (couldInsertInInner) {
                    currentSubSystems[i] = subSystem;
                    couldInsert = true;
                    return;
                }
            }

            couldInsert = false;
        }
    }

    private static string PrintSystemToString(PlayerLoopSystem s) {
        List<(PlayerLoopSystem, int)> systems = new List<(PlayerLoopSystem, int)>();

        AddRecursively(s, 0);
        void AddRecursively(PlayerLoopSystem system, int depth)
        {
            systems.Add((system, depth));
            if (system.subSystemList != null)
                foreach (var subsystem in system.subSystemList)
                    AddRecursively(subsystem, depth + 1);
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Systems");
        sb.AppendLine("=======");
        foreach (var (system, depth) in systems)
        {
            // root system has a null type, all others has a marker type.
            Append($"System Type: {system.type?.Name ?? "NULL"}");

            // This is a C# delegate, so it's only set for functions created on the C# side.
            Append($"Delegate: {system.updateDelegate}");

            // This is a pointer, probably to the function getting run internally. Has long values (like 140700263204024) for the builtin ones concrete ones,
            // while the builtin grouping functions has 0. So UnityEngine.PlayerLoop.Update has 0, while UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate
            // has a concrete value.
            Append($"Update Function: {system.updateFunction}");

            // The loopConditionFunction seems to be a red herring. It's set to a value for only UnityEngine.PlayerLoop.FixedUpdate, but setting a different
            // system to have the same loop condition function doesn't seem to do anything
            Append($"Loop Condition Function: {system.loopConditionFunction}");

            // null rather than an empty array when it's empty.
            Append($"{system.subSystemList?.Length ?? 0} subsystems");

            void Append(string s)
            {
                for (int i = 0; i < depth; i++)
                    sb.Append("  ");
                sb.AppendLine(s);
            }
        }

        return sb.ToString();
    }

    // [MenuItem("Test/Output current state to file")]
    private static void OutputCurrentStateToFile()
    {
        EnsureSystemFetched();
        var str = PrintSystemToString(rootSystem);

        Debug.Log(str);
        File.WriteAllText("playerLoopInterfaceOutput.txt", str);
    }
}
