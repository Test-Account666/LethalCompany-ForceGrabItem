using System.Linq;
using BepInEx.Bootstrap;

namespace ForceGrabItem;

internal static class DependencyChecker {
    internal static bool IsBetterItemHandlingInstalled() =>
        Chainloader.PluginInfos.Values.Any(metadata =>
                                               metadata.Metadata.GUID.Contains("Yan01h.BetterItemHandling"));

    internal static bool IsTelevisionControllerInstalled() =>
        Chainloader.PluginInfos.Values.Any(metadata =>
                                               metadata.Metadata.GUID.Contains("KoderTech.TelevisionController"));
}