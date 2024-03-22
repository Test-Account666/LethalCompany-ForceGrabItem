using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Mono.Collections.Generic;

namespace ForceGrabItem;

[HarmonyPatch(typeof(MenuManager))]
public static class MenuManagerPatch {
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void CheckForGrabObjectPatches() {
        var patches = Harmony.GetPatchInfo(AccessTools.DeclaredMethod(typeof(PlayerControllerB),
            nameof(PlayerControllerB.BeginGrabObject)));

        if (patches == null)
            return;

        var allPatches = new List<Patch>();

        allPatches.AddRange(patches.Finalizers ?? Enumerable.Empty<Patch>());
        allPatches.AddRange(patches.Postfixes ?? Enumerable.Empty<Patch>());
        allPatches.AddRange(patches.Prefixes ?? Enumerable.Empty<Patch>());
        allPatches.AddRange(patches.Transpilers ?? Enumerable.Empty<Patch>());
        allPatches.AddRange(patches.ILManipulators ?? Enumerable.Empty<Patch>());

        if (allPatches.Count <= 0)
            return;

        ForceGrabItem.Logger.LogWarning("Detected mods patching the PlayerControllerB#BeginGrabObject method!");
        ForceGrabItem.Logger.LogWarning("These mods using may not work correctly!");
        ForceGrabItem.Logger.LogWarning("Please report any issues!");


        HashSet<string> patchOwnerSet = [];

        foreach (var allPatch in allPatches)
            patchOwnerSet.Add(allPatch.owner);

        ForceGrabItem.Logger.LogWarning("Mods that might not work as expected:");

        foreach (var patchOwner in patchOwnerSet)
            ForceGrabItem.Logger.LogWarning(patchOwner);
    }
}