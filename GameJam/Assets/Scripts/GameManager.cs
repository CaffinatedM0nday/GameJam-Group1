using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;
    public TMP_Text[] statementTexts; // 3 statements
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

    [Header("Platform Tags")]
    public string easyFloorTag = "EasyPlatform";
    public string mediumFloorTag = "MediumPlatform";
    public string hardFloorTag = "HardPlatform";
    public string endPlatformTag = "EndPlatform";

    // Game state
    public Dictionary<int, List<GameObject>> floorPlatformsCache = new Dictionary<int, List<GameObject>>();
    public List<GameObject> currentPlatforms = new List<GameObject>();
    public float currentTime;
    public bool gameRunning;
    public int currentFloor = 0; // 0-Easy, 1-Medium, 2-Hard
    public Vector3 checkpointPosition;
    public List<string> safeColors = new List<string>(); // Which colors are safe this level

    private void Awake()
    {
        ValidateReferences();
        CachePlatforms();
    }

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
        nextLevelButton.onClick.AddListener(NextLevel);
        ShowStartScreen();
    }

    private void Update()
    {
        if (gameRunning)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTime <= 0)
            {
                GameOver();
            }
        }
    }

    private void ValidateReferences()
    {
        if (player == null) Debug.LogError("Player reference not set in GameManager!");
        if (floorSpawnPoints.Length < 3) Debug.LogError("Need 3 floor spawn points in GameManager!");
        if (statementTexts.Length < 3) Debug.LogError("Need 3 statement text fields in GameManager!");
    }

    private void CachePlatforms()
    {
        floorPlatformsCache.Clear();
        floorPlatformsCache[0] = new List<GameObject>(GameObject.FindGameObjectsWithTag(easyFloorTag));
        floorPlatformsCache[1] = new List<GameObject>(GameObject.FindGameObjectsWithTag(mediumFloorTag));
        floorPlatformsCache[2] = new List<GameObject>(GameObject.FindGameObjectsWithTag(hardFloorTag));

        // Cache end platforms if they exist
        GameObject[] endPlatforms = GameObject.FindGameObjectsWithTag(endPlatformTag);
        foreach (var platform in endPlatforms)
        {
            // Determine which floor this end platform belongs to
            for (int i = 0; i < 3; i++)
            {
                if (Vector3.Distance(platform.transform.position, floorSpawnPoints[i].position) < 20f)
                {
                    floorPlatformsCache[i].Add(platform);
                    break;
                }
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
    }

    private void SetupFloor(int floorLevel)
    {
        currentPlatforms = new List<GameObject>(floorPlatformsCache[floorLevel]);

        // Generate puzzle rules and process platforms
        GeneratePuzzleRules();
        floorText.text = $"Floor: {(floorLevel == 0 ? "Easy" : floorLevel == 1 ? "Medium" : "Hard")}";
        ProcessPlatforms();
    }

    private void ProcessPlatforms()
    {
        // First, reset all platforms
        foreach (var platform in currentPlatforms)
        {
            Collider platformCollider = platform.GetComponent<Collider>();
            if (platformCollider != null)
            {
                platformCollider.enabled = true;
            }

            // Reset platform state
            PlatformState state = platform.GetComponent<PlatformState>() ?? platform.AddComponent<PlatformState>();
            state.isDisabled = false;
            state.isSafe = false;
            platform.SetActive(true);
        }

        // Group platforms by color
        Dictionary<string, List<GameObject>> colorGroups = new Dictionary<string, List<GameObject>>()
        {
            {"GREEN", new List<GameObject>()},
            {"RED", new List<GameObject>()},
            {"YELLOW", new List<GameObject>()}
        };

        // Categorize platforms by color (skip end platform)
        foreach (var platform in currentPlatforms)
        {
            if (platform.CompareTag(endPlatformTag)) continue;

            string colorName = GetPlatformColor(platform);
            if (colorGroups.ContainsKey(colorName))
            {
                colorGroups[colorName].Add(platform);
            }
        }

        // Ensure at least one safe platform per color exists
        foreach (var colorGroup in colorGroups)
        {
            if (colorGroup.Value.Count > 0)
            {
                // If this is a safe color, ensure at least one platform remains active
                if (safeColors.Contains(colorGroup.Key))
                {
                    // Mark first platform as safe
                    PlatformState state = colorGroup.Value[0].GetComponent<PlatformState>() ??
                                       colorGroup.Value[0].AddComponent<PlatformState>();
                    state.isSafe = true;
                }
                else
                {
                    // For unsafe colors, leave a small chance for one platform to be safe
                    if (Random.Range(0f, 1f) < 0.2f) // 20% chance
                    {
                        PlatformState state = colorGroup.Value[0].GetComponent<PlatformState>() ??
                                           colorGroup.Value[0].AddComponent<PlatformState>();
                        state.isSafe = true;
                    }
                }
            }
        }
    }

    public void OnPlatformLanded(GameObject platform)
    {
        PlatformState state = platform.GetComponent<PlatformState>();

        // Check if platform is false and not already disabled
        if (state != null && !state.isSafe && !state.isDisabled)
        {
            StartCoroutine(DisablePlatformTemporarily(platform));
            return;
        }

        // Check if this is the end platform
        if (platform.CompareTag(endPlatformTag))
        {
            if (currentFloor < 2)
            {
                // Advance to next floor
                currentFloor++;
                currentTime = levelTime;
                SetupFloor(currentFloor);
                player.Respawn(floorSpawnPoints[currentFloor].position);
                checkpointPosition = floorSpawnPoints[currentFloor].position;
            }
            else
            {
                // Won the game
                GameWin();
            }
        }
    }

    private IEnumerator DisablePlatformTemporarily(GameObject platform)
    {
        PlatformState state = platform.GetComponent<PlatformState>();
        if (state == null || state.isDisabled) yield break;

        state.isDisabled = true;
        Collider platformCollider = platform.GetComponent<Collider>();
        if (platformCollider != null) platformCollider.enabled = false;

        // Visual feedback
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.gray;
            yield return new WaitForSeconds(falsePlatformReactivateTime);
            renderer.material.color = originalColor;
        }
        else
        {
            yield return new WaitForSeconds(falsePlatformReactivateTime);
        }

        if (platformCollider != null) platformCollider.enabled = true;
        state.isDisabled = false;
    }

    private void GeneratePuzzleRules()
    {
        safeColors.Clear();

        // Choose 1-2 safe colors (ensuring at least one)
        int safeCount = Random.Range(1, 3); // 1 or 2 safe colors
        List<string> allColors = new List<string> { "GREEN", "RED", "YELLOW" };

        // Select safe colors
        for (int i = 0; i < safeCount; i++)
        {
            int randomIndex = Random.Range(0, allColors.Count);
            safeColors.Add(allColors[randomIndex]);
            allColors.RemoveAt(randomIndex);
        }

        // Generate 3 statements (1 true, 2 false)
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
            // Generate true statement
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
            // Generate false statement with failsafe
            string falseStatement;
            int attempts = 0;

            do
            {
                attempts++;
                if (attempts > 100)
                {
                    return $"You CAN touch {GetRandomColorExcept(safeColors)}"; // Fallback
                }

                int statementType = Random.Range(0, 3);
                switch (statementType)
                {
                    case 0: // Wrong single color
                        falseStatement = $"You CAN touch {GetRandomColorExcept(safeColors)}";
                        break;

                    case 1: // Wrong "can't" statement
                        falseStatement = $"You CAN'T touch {safeColors[Random.Range(0, safeColors.Count)]}";
                        break;

                    case 2: // Wrong combination
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
        return "GREEN"; // Default
    }

    private bool IsColorSimilar(Color a, Color b)
    {
        return Vector4.Distance(a, b) < colorSimilarityThreshold;
    }

    public void PlayerFell()
    {
        player.Respawn(checkpointPosition);
        currentTime -= Mathf.Max(2, 7 - currentFloor * 2); // 5s on easy, 3s on medium, 1s on hard
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

    private void NextLevel()
    {
        RestartGame(); // Or load a specific next level
    }
}