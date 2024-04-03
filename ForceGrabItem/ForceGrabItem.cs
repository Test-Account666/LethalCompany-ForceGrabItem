using BepInEx;
using BepInEx.Logging;
using ForceGrabItem.Patches;
using HarmonyLib;

namespace ForceGrabItem;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("Yan01h.BetterItemHandling", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("KoderTech.TelevisionController", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.rune580.LethalCompanyInputUtils")]
public class ForceGrabItem : BaseUnityPlugin {
    public static ForceGrabItem Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        Patch();

        if (DependencyChecker.IsBetterItemHandlingInstalled()) {
            Logger.LogInfo("Found BetterItemHandling enabling support :)");
            BetterItemHandlingSupport.Setup();
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch() {
        Harmony ??= new(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll(typeof(PlayerControllerBPatch));

        if (DependencyChecker.IsTelevisionControllerInstalled())
            Harmony.PatchAll(typeof(TelevisionControllerPatch));

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}