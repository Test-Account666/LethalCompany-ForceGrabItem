using GameNetcodeStuff;
using HarmonyLib;
using Television_Controller;

namespace ForceGrabItem.Patches;

[HarmonyPatch(typeof(TelevisionController))]
public static class TelevisionControllerPatch {
    [HarmonyPatch(nameof(TelevisionController.SetHoverTipAndCurrentInteractTrigger))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static bool BeforeSetHoverTipAndCurrentInteractTrigger(object[] __args) =>
        PlayerControllerBPatch.HandleSetHoverTipAndCurrentInteractTrigger((PlayerControllerB) __args[0]);
}