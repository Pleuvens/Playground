using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    public void OnAttack(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            // Play animation
            Debug.Log("Attacking");
        }
    }
}
