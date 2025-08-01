using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    public float Movementspeed;
    public float JumpHeight;
    public GameManager gameManager;
    public float rotationSpeed = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        Movement();
        Rotation();
       
    }

    void Movement()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");

        Vector3 MovementDirection = new Vector3(Horizontal, 0, Vertical);
        transform.Translate(MovementDirection * Time.deltaTime * Movementspeed);

        // Jumping // 
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
        }
    }

    void Rotation()
    {
        // Get mouse movement
        float mouseX = Input.GetAxis("Mouse X");

        // Rotate player based on mouse movement
        transform.Rotate(Vector3.up, mouseX * rotationSpeed);
    }
    public void FallToVoid()
    {
        // Add falling animation/effects here
        Respawn(gameManager.checkpointPosition);
    }

    public void Respawn(Vector3 position)
    {
        transform.position = position;
        rb.velocity = Vector3.zero;
        gameManager.PlayerFell();
    }
}

