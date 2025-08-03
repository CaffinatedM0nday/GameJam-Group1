using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIScript : MonoBehaviour
{
    // Start is called before the first frame update
    public void start()
    {
        SceneManager.LoadScene("GameJam");
    }

    public void restart()
    {
        SceneManager.LoadScene("GameJam");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
