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
    public TMP_Text[] statementTexts;
    public TMP_Text timerText;
    public TMP_Text floorText;
    public Button startButton;
    public Button restartButton;

    [Header("Game Settings")]
    public float levelTime = 60f;
    public Transform[] floorSpawnPoints; // [0]Easy, [1]Medium, [2]Hard
    public GameObject platformPrefab;
    public float platformSpacing = 2f;
    public PlayerMovement player;

    [Header("Platform Colors")]
    public Material greenMat;
    public Material redMat;
    public Material yellowMat;

    public List<GameObject> currentPlatforms = new List<GameObject>();
    public Color currentSafeColor;
    public float currentTime;
    public bool gameRunning;
    public int currentFloor = 0; // 0-Easy, 1-Medium, 2-Hard
    public Vector3 checkpointPosition;

    public void Start()
    {
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
        ShowStartScreen();
    }

    public void Update()
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

    public void StartGame()
    {
        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        gameOverPanel.SetActive(false);

        currentFloor = 0;
        currentTime = levelTime;
        gameRunning = true;

        SetupFloor(currentFloor);
        player.Respawn(floorSpawnPoints[currentFloor].position);
        checkpointPosition = floorSpawnPoints[currentFloor].position;
    }

    void SetupFloor(int floorLevel)
    {
        // Clear existing platforms
        foreach (var platform in currentPlatforms)
        {
            Destroy(platform);
        }
        currentPlatforms.Clear();

        // Set floor parameters based on difficulty
        int gridSize = 3 + floorLevel; // 3x3, 4x4, 5x5
        float startOffset = -(gridSize - 1) * platformSpacing * 0.5f;

        // Create platform grid
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = floorSpawnPoints[floorLevel].position +
                                  new Vector3(startOffset + x * platformSpacing,
                                             1.5f + y * 0.5f,
                                             0);

                GameObject platform = Instantiate(platformPrefab, position, Quaternion.identity);
                currentPlatforms.Add(platform);

                // Assign random color
                int colorChoice = Random.Range(0, 3);
                Renderer platformRenderer = platform.GetComponent<Renderer>();

                switch (colorChoice)
                {
                    case 0:
                        platformRenderer.material = greenMat;
                        platform.tag = "Green";
                        break;
                    case 1:
                        platformRenderer.material = redMat;
                        platform.tag = "Red";
                        break;
                    case 2:
                        platformRenderer.material = yellowMat;
                        platform.tag = "Yellow";
                        break;
                }
            }
        }

        // Generate puzzle rules
        GeneratePuzzleRules();
        floorText.text = $"Floor: {(floorLevel == 0 ? "Easy" : floorLevel == 1 ? "Medium" : "Hard")}";
    }

    void GeneratePuzzleRules()
    {
        // Determine safe color
        Color[] colors = { Color.green, Color.red, Color.yellow };
        currentSafeColor = colors[Random.Range(0, colors.Length)];

        // Generate 3 statements (1 true, 2 false)
        int trueIndex = Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            bool isTrue = (i == trueIndex);
            string statement = GenerateStatement(isTrue);
            statementTexts[i].text = statement;
            statementTexts[i].color = isTrue ? Color.green : Color.red;
        }
    }

    string GenerateStatement(bool isTrue)
    {
        string[] templates = {
            "You CAN touch {0}",
            "You CAN'T touch {0}",
            "You CAN touch {0} and {1}",
            "You CAN'T touch {0} and {1}"
        };

        string template = templates[Random.Range(0, templates.Length)];
        bool isMultiColor = template.Contains("{1}");

        if (isTrue)
        {
            if (isMultiColor)
            {
                return string.Format(template, GetColorName(currentSafeColor), GetColorName(currentSafeColor));
            }
            else
            {
                if (template.Contains("CAN'T"))
                {
                    Color forbidden = GetRandomColorExcept(currentSafeColor);
                    return string.Format(template, GetColorName(forbidden));
                }
                else
                {
                    return string.Format(template, GetColorName(currentSafeColor));
                }
            }
        }
        else // False statement
        {
            if (isMultiColor)
            {
                Color color1 = Random.value > 0.5f ? currentSafeColor : GetRandomColorExcept(currentSafeColor);
                Color color2 = (color1 == currentSafeColor) ? GetRandomColorExcept(currentSafeColor) :
                             (Random.value > 0.5f ? currentSafeColor : GetRandomColorExcept(currentSafeColor));

                return string.Format(template, GetColorName(color1), GetColorName(color2));
            }
            else
            {
                if (template.Contains("CAN'T"))
                {
                    return string.Format(template, GetColorName(currentSafeColor));
                }
                else
                {
                    Color falseColor = GetRandomColorExcept(currentSafeColor);
                    return string.Format(template, GetColorName(falseColor));
                }
            }
        }
    }

    public void OnPlatformLanded(GameObject platform)
    {
        string platformTag = platform.tag;
        string safeColorName = GetColorName(currentSafeColor);

        if (platformTag == safeColorName)
        {
            // Correct platform
            if (IsCheckpointPlatform(platform))
            {
                AdvanceToNextFloor();
            }
        }
        else
        {
            // Wrong platform - player falls
            player.FallToVoid();
        }
    }

    bool IsCheckpointPlatform(GameObject platform)
    {
        // The highest platform in each layer acts as checkpoint
        float maxHeight = -Mathf.Infinity;
        GameObject highestPlatform = null;

        foreach (var p in currentPlatforms)
        {
            if (p.transform.position.y > maxHeight)
            {
                maxHeight = p.transform.position.y;
                highestPlatform = p;
            }
        }

        return platform == highestPlatform;
    }

    void AdvanceToNextFloor()
    {
        currentFloor++;
        if (currentFloor > 2) currentFloor = 2; // Cap at hard difficulty

        checkpointPosition = floorSpawnPoints[currentFloor].position;
        SetupFloor(currentFloor);
        currentTime = levelTime; // Reset timer for new floor
    }

    public void PlayerFell()
    {
        player.Respawn(checkpointPosition);
        currentTime -= 5f; // Time penalty
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    string GetColorName(Color color)
    {
        if (color == Color.green) return "GREEN";
        if (color == Color.red) return "RED";
        return "YELLOW";
    }

    Color GetRandomColorExcept(Color exception)
    {
        Color[] colors = { Color.green, Color.red, Color.yellow };
        List<Color> available = new List<Color>(colors);
        available.Remove(exception);
        return available[Random.Range(0, available.Count)];
    }

    void ShowStartScreen()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    void GameOver()
    {
        gameRunning = false;
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}