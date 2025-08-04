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
