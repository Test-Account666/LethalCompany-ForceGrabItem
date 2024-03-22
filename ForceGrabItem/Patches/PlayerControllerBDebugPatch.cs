using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ForceGrabItem.Patches;

/*
 * Only uncomment for testing purposes
 */
//[HarmonyPatch(typeof(PlayerControllerB))]
public static class PlayerControllerBDebugPatch {
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AfterUpdate(PlayerControllerB __instance) {
        var isPressed = IngamePlayerSettings.Instance.playerInput.actions.FindAction("ItemSecondaryUse").IsPressed();

        if (!isPressed)
            return;

        var doorLock = Object.FindObjectOfType<DoorLock>();

        __instance.TeleportPlayer(doorLock.transform.position);
        __instance.isInsideFactory = true;
        __instance.isInHangarShipRoom = false;
        __instance.isInElevator = false;
    }
}