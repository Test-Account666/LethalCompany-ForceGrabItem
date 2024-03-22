using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace ForceGrabItem;

public class ForceGrabItemInputActions : LcInputActions {
    public static ForceGrabItemInputActions instance = new();

    [InputAction("<Keyboard>/leftShift", Name = "NormalGrab")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public InputAction NormalGrabKey { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}