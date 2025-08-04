using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDisable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check which color platform was touched
            if (gameObject.CompareTag("Red"))
            {
                FindObjectOfType<GameManager>().TF2();
                DisablePlatform();
                Debug.Log("Red platform disabled");
            }
            else if (gameObject.CompareTag("Yellow"))
            {
                DisablePlatform();
                Debug.Log("Yellow platform disabled");
            }
            else if (gameObject.CompareTag("Green"))
            {
                DisablePlatform();
                Debug.Log("Green platform disabled");
            }
        }
    }
    public void DisablePlatform()
    {
        // Disable the collider and renderer
        GetComponent<Collider>().enabled = false;
    }

    public void ReEnablePlatform()
    {

        // Re-enable the platform
        GetComponent<Collider>().enabled = true;
    }
}
