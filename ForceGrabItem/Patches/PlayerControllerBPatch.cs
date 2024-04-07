using System;
using System.Text.RegularExpressions;
using GameNetcodeStuff;
using HarmonyLib;
using MonoMod.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace ForceGrabItem.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public static class PlayerControllerBPatch {
    private static readonly int _GrabInvalidated = Animator.StringToHash("GrabInvalidated");
    private static readonly int _GrabValidated = Animator.StringToHash("GrabValidated");
    private static readonly int _CancelHolding = Animator.StringToHash("cancelHolding");
    private static readonly int _Throw = Animator.StringToHash("Throw");

    private static readonly Regex _DirtyInteractRegex = new("Interact:<(Keyboard|Mouse)>/", RegexOptions.Compiled);

    [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
    [HarmonyPrefix]
    [HarmonyBefore("com.kodertech.TelevisionController")]
    // ReSharper disable once InconsistentNaming
    public static bool BeforeSetHoverTipAndCurrentInteractTrigger(PlayerControllerB __instance) =>
        DependencyChecker.IsTelevisionControllerInstalled()
     || HandleSetHoverTipAndCurrentInteractTrigger(__instance);

    public static bool HandleSetHoverTipAndCurrentInteractTrigger(PlayerControllerB playerControllerB) {
        if (ForceGrabItemInputActions.instance.NormalGrabKey.IsPressed())
            return true;

        if (playerControllerB.hoveringOverTrigger != null && playerControllerB.hoveringOverTrigger.isBeingHeldByPlayer)
            return true;

        if (!IsLocalPlayer(playerControllerB) || playerControllerB.isGrabbingObjectAnimation)
            return true;

        if (!RaycastForObject(playerControllerB, out var hit)) {
            ClearTriggerAndTip(playerControllerB);
            return true;
        }

        if (playerControllerB.FirstEmptyItemSlot() == -1) {
            playerControllerB.cursorTip.text = "Inventory full!";
            return false;
        }

        var grabObject = hit.collider.GetComponent<GrabbableObject>();

        if (grabObject != null)
            playerControllerB.hoveringOverTrigger = null;

        if (grabObject != null && !string.IsNullOrEmpty(grabObject.customGrabTooltip)) {
            playerControllerB.cursorTip.text = grabObject.customGrabTooltip;
            return false;
        }

        var keyToPress = GetInteractKey();
        playerControllerB.cursorTip.text = $"Grab : [{keyToPress}]";
        playerControllerB.cursorIcon.enabled = true;
        playerControllerB.cursorIcon.sprite = playerControllerB.grabItemIcon;

        return false;
    }

    [HarmonyPatch("Interact_performed")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void Interact_performed(PlayerControllerB __instance, ref InputAction.CallbackContext context) {
        if (ForceGrabItemInputActions.instance.NormalGrabKey.IsPressed())
            return;

        if (__instance.hoveringOverTrigger != null && __instance.hoveringOverTrigger.isBeingHeldByPlayer)
            return;

        if (((!__instance.IsOwner || !__instance.isPlayerControlled ||
              __instance is { IsServer: true, isHostPlayerObject: false }) && !__instance.isTestingPlayer) ||
            !context.performed)
            return;

        BeginGrabObject(__instance);
    }

    private static void BeginGrabObject(PlayerControllerB instance) {
        ForceGrabItemHook.OnPreBeforeGrabObject(instance, null);

        if (!IsLocalPlayer(instance))
            return;

        if (!RaycastForObject(instance, out var hit))
            return;

        var grabObject = hit.collider.GetComponent<GrabbableObject>();

        ForceGrabItemHook.OnBeforeGrabObject(instance, grabObject);

        if (grabObject == null || instance.inSpecialInteractAnimation || instance.isGrabbingObjectAnimation ||
            grabObject.isHeld || grabObject.isPocketed)
            return;

        var networkObject = grabObject.NetworkObject;

        if (networkObject == null || !networkObject.IsSpawned)
            return;

        try {
            grabObject.InteractItem();
        } catch (Exception exception) {
            exception.LogDetailed();
        }

        if (instance.twoHanded)
            return;

        if (!grabObject.grabbable || instance.FirstEmptyItemSlot() == -1)
            return;

        ResetAnimators(instance);

        instance.currentlyGrabbingObject = grabObject;

        instance.SetSpecialGrabAnimationBool(true);
        instance.isGrabbingObjectAnimation = true;

        instance.cursorIcon.enabled = false;
        instance.cursorTip.text = "";

        instance.twoHanded = grabObject.itemProperties.twoHanded;

        instance.carryWeight += Mathf.Clamp(grabObject.itemProperties.weight - 1f, 0.0f, 10f);

        instance.grabObjectAnimationTime = grabObject.itemProperties.grabAnimationTime <= 0.0f
            ? 0.4f
            : grabObject.itemProperties.grabAnimationTime;

        if (!instance.isTestingPlayer)
            instance.GrabObjectServerRpc((NetworkObjectReference) networkObject);

        instance.grabObjectCoroutine = instance.StartCoroutine(instance.GrabObject());

        ForceGrabItemHook.OnAfterGrabObject(instance, grabObject);
    }

    private static bool IsLocalPlayer(Object player) =>
        player == StartOfRound.Instance.localPlayerController;


    private static bool RaycastForObject(PlayerControllerB player, out RaycastHit hit) {
        if (player == null || player.gameplayCamera == null) {
            hit = default;
            return false;
        }

        var cameraTransform = player.gameplayCamera.transform;

        var position = cameraTransform.position;
        var forward = cameraTransform.forward;

        var ray = new Ray(position, forward);

        // Raycast to detect grabbable objects
        var raycastHit = Physics.Raycast(ray, out hit, player.grabDistance, player.grabbableObjectsMask
                                                                          | (1 << 8) /* Walls, etc.*/
                                                                          | (1 << 25) /* Terrain */
                                                                          | (1 << 12) /* PhysicsObject */) &&
                         hit.collider.CompareTag("PhysicsProp");

        if (!raycastHit)
            return false;

        if (IsInteractableObjectHit(ray, player, hit))
            return false;

        // Check if there's a door obstructing the grabbable object
        return !IsDoorHit(ray, player, hit);
    }

#pragma warning disable Harmony003
    private static bool PerformRaycastAndCheckConditions(Ray ray, PlayerControllerB player, RaycastHit grabbableObjectHit,
                                                         bool checkDoor = true) {
        var raycastHit = Physics.Raycast(ray, out var hit, player.grabDistance, 1 << 9);

        if (!raycastHit)
            return false; // No hit, allow grabbing

        var doorLock = hit.collider.GetComponent<DoorLock>();

        if (checkDoor) {
            if (doorLock == null)
                return false; // No DoorLock component found, allow grabbing
        } else if (doorLock != null)
            return false; // DoorLock found, skipping

        var colliders = hit.collider.gameObject.GetComponents<BoxCollider>();

        foreach (var collider in colliders) {
            // A trigger allows the player to pass through
            // We only want physical colliders
            if (!collider || collider.isTrigger)
                continue;

            var originalSize = collider.size;

            // Modify collider size. The x value actually works against us if it is a door
            collider.size = new( /*checkDoor?*/ 0 /* : originalSize.x*/, originalSize.y, originalSize.z);

            var boxHit = collider.Raycast(ray, out var boxHitInfo, player.grabDistance);

            collider.size = originalSize; // Revert to original size

            if (!boxHit)
                continue;

            // Check if the object is closer than the grabbed object
            return boxHitInfo.distance < grabbableObjectHit.distance;
        }

        return false; // No collision with the object, allow grabbing
    }

    private static bool IsDoorHit(Ray ray, PlayerControllerB player, RaycastHit grabbableObjectHit) =>
        PerformRaycastAndCheckConditions(ray, player, grabbableObjectHit);

    private static bool IsInteractableObjectHit(Ray ray, PlayerControllerB player, RaycastHit grabbableObjectHit) =>
        PerformRaycastAndCheckConditions(ray, player, grabbableObjectHit, false);
#pragma warning restore Harmony003

    private static void ClearTriggerAndTip(PlayerControllerB playerControllerB) {
        playerControllerB.cursorIcon.enabled = false;
        playerControllerB.cursorTip.text = "";

        if (playerControllerB.hoveringOverTrigger != null)
            playerControllerB.previousHoveringOverTrigger = playerControllerB.hoveringOverTrigger;

        playerControllerB.hoveringOverTrigger = null;
    }

    private static void ResetAnimators(PlayerControllerB playerControllerB) {
        var animator = playerControllerB.playerBodyAnimator;
        animator.SetBool(_GrabInvalidated, false);
        animator.SetBool(_GrabValidated, false);
        animator.SetBool(_CancelHolding, false);
        animator.ResetTrigger(_Throw);
    }

    private static string GetInteractKey() {
        var interactAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact");
        var keyToPress = interactAction.bindings[0].ToString();
        return _DirtyInteractRegex.Replace(keyToPress, "").ToUpper();
    }
}