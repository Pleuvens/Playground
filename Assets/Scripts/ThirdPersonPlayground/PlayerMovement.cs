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
    #endregion

    #region Animation
    [SerializeField] Animator animator;
    #endregion


    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();
        direction = new Vector3(inputMovement.x, 0, inputMovement.y);
        animator.SetBool("run", !direction.Equals(Vector3.zero));
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        transform.Rotate(Vector3.up * direction.x * rotationSpeed * Time.deltaTime);
    }
}
