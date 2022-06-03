using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Movement attributes
    [SerializeField] Vector3 direction;
    [SerializeField] float speed;
    [SerializeField] float rotationSpeed;
    [SerializeField] bool isMovementFreezed = false;
    #endregion

    #region Animation
    [SerializeField] Animator animator;
    #endregion

    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();
        direction = new Vector3(inputMovement.x, 0, inputMovement.y);
        animator.SetBool("run", inputMovement.y != 0);
    }

    public void FreezeMovement()
    {
        isMovementFreezed = true;
    }

    public void UnfreezeMovement()
    {
        isMovementFreezed = false;
    }

    private void Update()
    {
        if (!isMovementFreezed)
        {
            transform.Translate(direction * speed * Time.deltaTime);
            transform.Rotate(Vector3.up * direction.x * rotationSpeed * Time.deltaTime);
        }
    }
}
