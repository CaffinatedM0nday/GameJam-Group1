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
    public AudioSource bgmAudio;

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

        currentFloor = 0;
        currentTime = levelTime;
        gameRunning = true;

        SetupFloor(currentFloor);
        player.Respawn(floorSpawnPoints[currentFloor].position);
        checkpointPosition = floorSpawnPoints[currentFloor].position;
        if (bgmAudio != null && !bgmAudio.isPlaying)
        {
            bgmAudio.Play();
        }
    }

    private void SetupFloor(int floorLevel)
    {
        currentPlatforms = new List<GameObject>(floorPlatformsCache[floorLevel]);
        GeneratePuzzleRules();
        floorText.text = $"Floor: {(floorLevel == 0 ? "Easy" : floorLevel == 1 ? "Medium" : "Hard")}";
        ProcessPlatforms();
    }

    private void ProcessPlatforms()
    {
        foreach (var platform in currentPlatforms)
        {
            // Try to get MeshCollider first, then fall back to any Collider
            MeshCollider meshCollider = platform.GetComponent<MeshCollider>();
            Collider anyCollider = meshCollider != null ? meshCollider : platform.GetComponent<Collider>();

            if (anyCollider != null)
            {
                anyCollider.enabled = true;
            }

            PlatformState state = platform.GetComponent<PlatformState>() ?? platform.AddComponent<PlatformState>();
            state.isDisabled = false;
            state.isSafe = IsPlatformSafe(platform);
        }
    }

    private bool IsPlatformSafe(GameObject platform)
    {
        if (platform.CompareTag(endPlatformTag)) return true;

        string platformColor = GetPlatformColor(platform);
        return safeColors.Contains(platformColor);
    }

    public void OnPlatformLanded(GameObject platform)
    {
        PlatformState state = platform.GetComponent<PlatformState>();
        string platformColor = GetPlatformColor(platform);

        if (ShouldDisablePlatform(platformColor, state))
        {
            DisablePlatformCollider(platform); // Disable collider immediately
            PlayerFell();
            return;
        }
    }

    // New method: Disable the platform's collider
    private void DisablePlatformCollider(GameObject platform)
    {
        // Try MeshCollider first, then fall back to any Collider
        MeshCollider meshCollider = platform.GetComponent<MeshCollider>();
        Collider anyCollider = meshCollider != null ? meshCollider : platform.GetComponent<Collider>();

        if (anyCollider != null)
        {
            anyCollider.enabled = false;
        }

        // Update platform state
        PlatformState state = platform.GetComponent<PlatformState>();
        if (state != null)
        {
            state.isDisabled = true;
        }
    }

    public bool ShouldDisablePlatform(string platformColor, PlatformState state)
    {
        return (state != null && !state.isSafe && !state.isDisabled &&
               !safeColors.Contains(platformColor));
    }

    

    public void PlayerFell()
    {
        if (isPlayerFalling) return;

        isPlayerFalling = true;
        currentTime -= Mathf.Max(2, 7 - currentFloor * 2);

        StartCoroutine(RespawnPlayerAfterDelay(1f));
    }

    private IEnumerator RespawnPlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        player.Respawn(checkpointPosition);
        isPlayerFalling = false;
    }

    private void AdvanceToNextFloor(int newFloor)
    {
        currentFloor = newFloor;
        currentTime = levelTime;
        checkpointPosition = floorSpawnPoints[currentFloor].position;

        SetupFloor(currentFloor);
        player.Respawn(checkpointPosition);
        
    }

    private void GeneratePuzzleRules()
    {
        safeColors.Clear();

        int safeCount = Random.Range(1, 3);
        List<string> allColors = new List<string> { "GREEN", "RED", "YELLOW" };

        for (int i = 0; i < safeCount; i++)
        {
            int randomIndex = Random.Range(0, allColors.Count);
            safeColors.Add(allColors[randomIndex]);
            allColors.RemoveAt(randomIndex);
        }

        int trueIndex = Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            bool isTrue = (i == trueIndex);
            statementTexts[i].text = GenerateStatement(isTrue);
            statementTexts[i].color = isTrue ? Color.green : Color.red;
        }
    }

    private string GenerateStatement(bool isTrue)
    {
        if (isTrue)
        {
            if (safeColors.Count == 1)
            {
                return $"You CAN touch {safeColors[0]}";
            }
            else
            {
                return $"You CAN touch {safeColors[0]} and {safeColors[1]}";
            }
        }
        else
        {
            string falseStatement;
            int attempts = 0;

            do
            {
                attempts++;
                if (attempts > 100)
                {
                    return $"You CAN touch {GetRandomColorExcept(safeColors)}";
                }

                int statementType = Random.Range(0, 3);
                switch (statementType)
                {
                    case 0:
                        falseStatement = $"You CAN touch {GetRandomColorExcept(safeColors)}";
                        break;
                    case 1:
                        falseStatement = $"You CAN'T touch {safeColors[Random.Range(0, safeColors.Count)]}";
                        break;
                    case 2:
                        string color1 = GetRandomColor();
                        string color2 = GetRandomColorExcept(new List<string> { color1 });
                        falseStatement = $"You CAN touch {color1} and {color2}";
                        break;
                    default:
                        falseStatement = $"You CAN touch {GetRandomColorExcept(safeColors)}";
                        break;
                }
            } while (IsStatementActuallyTrue(falseStatement));

            return falseStatement;
        }
    }

    private bool IsStatementActuallyTrue(string statement)
    {
        if (statement.Contains("CAN'T"))
        {
            string color = statement.Replace("You CAN'T touch ", "").Trim();
            return !safeColors.Contains(color);
        }
        else if (statement.Contains("and"))
        {
            string[] parts = statement.Replace("You CAN touch ", "").Split(new string[] { " and " }, System.StringSplitOptions.None);
            return safeColors.Contains(parts[0]) && safeColors.Contains(parts[1]);
        }
        else
        {
            string color = statement.Replace("You CAN touch ", "").Trim();
            return safeColors.Contains(color);
        }
    }

    private string GetPlatformColor(GameObject platform)
    {
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color platformColor = renderer.material.color;

            if (IsColorSimilar(platformColor, Color.green)) return "GREEN";
            if (IsColorSimilar(platformColor, Color.red)) return "RED";
            if (IsColorSimilar(platformColor, Color.yellow)) return "YELLOW";
        }
        return "GREEN";
    }

    private bool IsColorSimilar(Color a, Color b)
    {
        return Vector4.Distance(a, b) < colorSimilarityThreshold;
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private string GetRandomColor()
    {
        string[] colors = { "GREEN", "RED", "YELLOW" };
        return colors[Random.Range(0, colors.Length)];
    }

    private string GetRandomColorExcept(List<string> excludedColors)
    {
        List<string> available = new List<string> { "GREEN", "RED", "YELLOW" };
        foreach (var color in excludedColors)
        {
            available.Remove(color);
        }
        return available[Random.Range(0, available.Count)];

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