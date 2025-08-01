using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    public float Movementspeed;
    public float JumpHeight;

    float Horizontal = Input.GetAxis("Horizontal");
    float Vertical = Input.GetAxis("Vertical");

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        Movement();
       
    }

    void Movement()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Vector3 MovementDirection = new Vector3(Horizontal, 0, Vertical);
            transform.Translate(MovementDirection * Time.deltaTime * Movementspeed);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
        }
    }
}
