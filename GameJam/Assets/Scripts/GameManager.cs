using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elements")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;
    public TMP_Text[] statementTexts;
    public TMP_Text timerText;
    public TMP_Text floorText;
    public Button startButton;
    public Button restartButton;
    public Button nextLevelButton;

    [Header("Game Settings")]
    public float levelTime = 60f;
    public Transform[] floorSpawnPoints; // [0]Easy, [1]Medium, [2]Hard
    public PlayerMovement player;
    public float falsePlatformReactivateTime = 2f;
    public float colorSimilarityThreshold = 0.2f;
    private MeshCollider meshCollider;

    [Header("Platform Tags")]
    public string easyFloorTag = "EasyPlatform";
    public string mediumFloorTag = "MediumPlatform";
    public string hardFloorTag = "HardPlatform";
    public string endPlatformTag = "EndPlatform";

    // Game state
    private Dictionary<int, List<GameObject>> floorPlatformsCache = new Dictionary<int, List<GameObject>>();
    private List<GameObject> currentPlatforms = new List<GameObject>();
    private float currentTime;
    private bool gameRunning;
    private int currentFloor = 0;
    public Vector3 checkpointPosition;
    private List<string> safeColors = new List<string>();
    private bool isPlayerFalling = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
        ShowStartScreen();
        meshCollider = GetComponent<MeshCollider>();
    }

    private void Update()
    {
        if (gameRunning)
        {
            currentTime -= Time.deltaTime;
            

            if (currentTime <= 0)
            {
                GameOver();
            }
        }
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        gameOverPanel.SetActive(false);
        gameWinPanel.SetActive(false);
    }

    private void ShowStartScreen()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameWinPanel.SetActive(false);
    }

    private void GameOver()
    {
        gameRunning = false;
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    private void GameWin()
    {
        gameRunning = false;
        gamePanel.SetActive(false);
        gameWinPanel.SetActive(true);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RefreshStatements()
    {
        //GeneratePuzzleRules();
    }
    public void WinTrigger()
    {
        GameWin();   
    }
}