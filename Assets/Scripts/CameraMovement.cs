using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;

    void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        transform.position = Vector3.Lerp(transform.position, transform.position + movement, Time.deltaTime * moveSpeed / Time.timeScale);
    }
}
