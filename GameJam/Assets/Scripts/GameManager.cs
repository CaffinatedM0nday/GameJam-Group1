using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;

    [Header("Timer Settings")]
    public float timeRemaining = 60f;
    public bool timerIsRunning = false;
    public TMP_Text timeText; // Changed to TMP_Text

    [Header("Buttons")]
    public Button startButton;
    public Button restartButton;

    private void Start()
    {
        // Set up button listeners
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);

        // Initialize UI
        ShowStartScreen();
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                GameOver();
            }
        }
    }

    void UpdateTimerDisplay()
    {
        // Calculate minutes and seconds
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // Update TMP text
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void ShowStartScreen()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        timerIsRunning = false;
    }

    public void StartGame()
    {
        // Reset timer
        timeRemaining = 60f;
        UpdateTimerDisplay();

        // Switch panels
        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        gameOverPanel.SetActive(false);

        // Start timer
        timerIsRunning = true;

        // Add your game initialization code here
        Debug.Log("Game Started!");
    }

    public void GameOver()
    {
        timerIsRunning = false;
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);

        // Add your game over logic here
        Debug.Log("Game Over!");
    }

    public void RestartGame()
    {
        // Option 1: Reload the scene (simpler)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Option 2: Reset manually without reloading
        /*
        timeRemaining = 60f;
        UpdateTimerDisplay();
        ShowStartScreen();
        */
    }
}