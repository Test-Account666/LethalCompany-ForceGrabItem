using System.Linq;
using BepInEx.Bootstrap;

namespace ForceGrabItem;

internal static class DependencyChecker {
    internal static bool IsBetterItemHandlingInstalled() {
        return Chainloader.PluginInfos.Values.Any(metadata =>
            metadata.Metadata.GUID.Contains("Yan01h.BetterItemHandling"));
    }
}