using System;
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
        transform.position += newInput().normalized * (currentMoveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.Mouse2))
            HandleMouseInput();
    }

    Vector3 newInput()
    {
        Vector3 input = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
            input -= transform.right;
        if (Input.GetKey(KeyCode.D))
            input += transform.right;
        if (Input.GetKey(KeyCode.W))
            input += transform.forward;
        if (Input.GetKey(KeyCode.S))
            input -= transform.forward;
        if (Input.GetKey(KeyCode.Space))
            input += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl))
            input += Vector3.down;
        return input;
    }

    void HandleMouseInput()
    {
        transform.rotation *= Quaternion.AngleAxis(
            -Input.GetAxis("Mouse Y") * _rotationSpeed,
            Vector3.right
        );

        Vector3 eulerAngles = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(
            eulerAngles.x,
            eulerAngles.y + Input.GetAxis("Mouse X") * _rotationSpeed,
            eulerAngles.z
        );
    }
}
