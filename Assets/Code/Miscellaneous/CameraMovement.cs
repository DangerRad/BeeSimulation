using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float _moveSpeed;
    [SerializeField] float _sprintSpeed;
    [SerializeField] float _rotationSpeed;

    void Update()
    {
        Move();
    }
    void Move()
    {
        float currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? _sprintSpeed : _moveSpeed;

        float verticalInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
        verticalInput = Input.GetKey(KeyCode.LeftShift) ? -1 : verticalInput;

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), verticalInput, Input.GetAxisRaw("Vertical"));
        transform.position += input.normalized * (currentMoveSpeed * Time.deltaTime);
        // transform.position += transform.forward.normalized * currentMoveSpeed * Time.deltaTime * Input.GetAxisRaw("Vertical");

        float rotationInput = Input.GetKey(KeyCode.Q) ? -1 : 0;
        rotationInput = Input.GetKey(KeyCode.E) ? 1 : rotationInput;

        Vector3 rotation = transform.eulerAngles;
        rotation.y += rotationInput * _rotationSpeed;
        transform.eulerAngles = rotation;
    }

}
