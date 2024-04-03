using System;
using System.Linq;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;

namespace ForceGrabItem;

public static class BetterItemHandlingSupport {
    private static Assembly? _betterItemHandlingAssembly;
    private static Type? _playerControllerBPatchesType;
    private static MethodInfo? _beginGrabObjectPrefixMethod;
    private static MethodInfo? _beginGrabObjectPostfixMethod;

    public static void Setup() {
        ForceGrabItemHook.BeforeGrabObject += BeforeBeginGrabObject;
        ForceGrabItemHook.AfterGrabObject += AfterBeginGrabObject;
    }

    private static void BeforeBeginGrabObject(GrabObjectEventArgs grabObjectEventArgs) {
        if (!FindBetterItemHandlingAssembly())
            return;

        if (!FindPlayerControllerPatchType())
            return;

        if (!FindBeginGrabObjectPrefixMethod())
            return;

        _beginGrabObjectPrefixMethod?.Invoke(null, [
            grabObjectEventArgs.playerControllerB,
        ]);
    }

    private static void AfterBeginGrabObject(GrabObjectEventArgs grabObjectEventArgs) {
        if (!FindBetterItemHandlingAssembly())
            return;

        if (!FindPlayerControllerPatchType())
            return;

        if (!FindBeginGrabObjectPostfixMethod())
            return;

        _beginGrabObjectPostfixMethod?.Invoke(null, [
            grabObjectEventArgs.playerControllerB,
        ]);
    }

    private static bool FindBetterItemHandlingAssembly() {
        if (_betterItemHandlingAssembly != null)
            return true;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            if (!assembly.FullName.ToLower().Contains("betteritemhandling"))
                continue;

            _betterItemHandlingAssembly = assembly;
            break;
        }

        if (_betterItemHandlingAssembly != null)
            return true;

        ForceGrabItem.Logger.LogError("Couldn't find BetterItemHandling assembly!");
        return false;
    }

    private static bool FindPlayerControllerPatchType() {
        if (_playerControllerBPatchesType is not null)
            return true;

        _playerControllerBPatchesType = AccessTools.GetTypesFromAssembly(_betterItemHandlingAssembly)
                                                   .Where((Func<Type, bool>) (type =>
                                                              type.FullName is
                                                                  "BetterItemHandling.Patches.PlayerControllerBPatches"))
                                                   .FirstOrDefault();

        if (_playerControllerBPatchesType is not null)
            return true;

        ForceGrabItem.Logger.LogError("Couldn't find BetterItemHandling PlayerControllerBPatches type!");
        return false;
    }

    private static bool FindBeginGrabObjectPrefixMethod() {
        if (_beginGrabObjectPrefixMethod is not null)
            return true;

        _beginGrabObjectPrefixMethod =
            AccessTools.DeclaredMethod(_playerControllerBPatchesType, "BeginGrabObjectPrefix",
            [
                typeof(PlayerControllerB),
            ]);

        if (_beginGrabObjectPrefixMethod is not null)
            return true;

        ForceGrabItem.Logger.LogError("Couldn't find BetterItemHandling BeginGrabObjectPrefix method!");
        return false;
    }

    private static bool FindBeginGrabObjectPostfixMethod() {
        if (_beginGrabObjectPostfixMethod is not null)
            return true;

        _beginGrabObjectPostfixMethod =
            AccessTools.DeclaredMethod(_playerControllerBPatchesType, "BeginGrabObjectPostfix",
            [
                typeof(PlayerControllerB),
            ]);

        if (_beginGrabObjectPostfixMethod is not null)
            return true;

        ForceGrabItem.Logger.LogError("Couldn't find BetterItemHandling BeginGrabObjectPostfix method!");
        return false;
    }
}