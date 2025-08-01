using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_Text TrueOrFalse1;
    public TMP_Text TrueOrFalse2;
    public TMP_Text TrueOrFalse3;
    public TMP_Text Timer;
    public float timeRemaining = 60f;
    public bool timerIsRunning = false;
    // Start is called before the first frame update
    void Start()
    {
        timerIsRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
                // You can add code here to execute when timer finishes
            }
        }
    }
    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1; // Add 1 to show the full second as it counts down

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        Timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
