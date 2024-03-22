using System;
using GameNetcodeStuff;

namespace ForceGrabItem;

public class GrabObjectEventArgs(PlayerControllerB playerControllerB, GrabbableObject? grabbableObject) : EventArgs {
    public readonly PlayerControllerB playerControllerB = playerControllerB;
    public GrabbableObject? grabbableObject = grabbableObject;
}