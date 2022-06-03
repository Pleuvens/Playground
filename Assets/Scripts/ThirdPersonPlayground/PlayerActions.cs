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

    private void Start()
    {
        AddAnimationEvent(3, true, "OnBlockingAnimationStart", 0);
        AddAnimationEvent(3, false, "OnBlockingAnimationEnd", 0);
    }

    void OnBlockingAnimationStart()
    {
        playerMovement.FreezeMovement();
    }

    void OnBlockingAnimationEnd()
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
