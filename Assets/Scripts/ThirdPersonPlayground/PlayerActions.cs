using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    #region Animation
    [SerializeField] Animator animator;
    #endregion

    [SerializeField] PlayerMovement playerMovement;

    private const string blockingStartFnName = "DisableMovement";
    private const string blockingEndFnName = "EnableMovement";

    private void Start()
    {
        AddAnimationEvent(PlayerClip.Attack, true, blockingStartFnName, 0);
        AddAnimationEvent(PlayerClip.Attack, false, blockingEndFnName, 0);
        AddAnimationEvent(PlayerClip.Pickup, true, blockingStartFnName, 0);
        AddAnimationEvent(PlayerClip.Pickup, false, blockingEndFnName, 0);
    }

    void DisableMovement()
    {
        playerMovement.FreezeMovement();
    }

    void EnableMovement()
    {
        playerMovement.UnfreezeMovement();
    }

    public void OnAttack(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            animator.SetBool("attack_1", true);
        }
    }

    public void OnPickup(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            animator.SetBool("tr_pickup", true);
        }
    }

    public void InteractWithItem()
    {
        Debug.Log("Picked up");
    }

    void AddAnimationEvent(int clip, bool start, string functionName, float floatParam)
    {
        AnimationClip animClip = animator.runtimeAnimatorController.animationClips[clip];
        AnimationEvent animEvent = new AnimationEvent();
        animEvent.functionName = functionName;
        animEvent.floatParameter = floatParam;
        animEvent.time = start ? 0f : animClip.length;
        animClip.AddEvent(animEvent);
    }
}
