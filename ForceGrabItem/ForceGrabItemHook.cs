using GameNetcodeStuff;

namespace ForceGrabItem;

public static class ForceGrabItemHook {
    public delegate void GrabObjectEvent(GrabObjectEventArgs args);

    internal static void OnPreBeforeGrabObject(PlayerControllerB playerControllerB, GrabbableObject? grabbableObject) {
        ForceGrabItem.Logger.LogDebug("OnPreBeforeGrabObject!");
        PreBeforeGrabObject?.Invoke(new GrabObjectEventArgs(playerControllerB, grabbableObject));
        ForceGrabItem.Logger.LogDebug(PreBeforeGrabObject != null);
    }

    internal static void OnBeforeGrabObject(PlayerControllerB playerControllerB, GrabbableObject? grabbableObject) {
        ForceGrabItem.Logger.LogDebug("OnBeforeGrabObject!");
        BeforeGrabObject?.Invoke(new GrabObjectEventArgs(playerControllerB, grabbableObject));
        ForceGrabItem.Logger.LogDebug(BeforeGrabObject != null);
    }

    internal static void OnAfterGrabObject(PlayerControllerB playerControllerB, GrabbableObject? grabbableObject) {
        ForceGrabItem.Logger.LogDebug("OnAfterGrabObject!");
        AfterGrabObject?.Invoke(new GrabObjectEventArgs(playerControllerB, grabbableObject));
        ForceGrabItem.Logger.LogDebug(AfterGrabObject != null);
    }
    // ReSharper disable once EventNeverSubscribedTo.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static event GrabObjectEvent PreBeforeGrabObject;
    public static event GrabObjectEvent BeforeGrabObject;
    public static event GrabObjectEvent AfterGrabObject;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}