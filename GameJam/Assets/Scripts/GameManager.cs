using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
public enum PlatFormState
{
    TF1,
    TF2,
    TF3,
    TF4,
    TF5,
    TF6,
    TF7,
    TF8,
    TF9,
    TF10,

}

// Main game manager class that controls game flow and logic
public class GameManager : MonoBehaviour
{
    // Singleton pattern implementation
    public static GameManager Instance;

    // UI Elements section
    [Header("UI Elements")]
    public TMP_Text[] statementTexts;     // Text elements for puzzle statements
    public TMP_Text timerText;            // Text element for timer display


    // Game Settings section
    [Header("Game Settings")]
    public float levelTime = 60f;                         // Total time per level
    public Transform[] floorSpawnPoints;                  // Spawn points for each floor difficulty
    public PlayerMovement player;                         // Reference to player object
    public float falsePlatformReactivateTime = 2f;        // Time before false platforms reactivate
    public float colorSimilarityThreshold = 0.2f;         // Threshold for color comparison
    private MeshCollider meshCollider;                    // Reference to mesh collider


    // Game state variables
    private Dictionary<int, List<GameObject>> floorPlatformsCache = new Dictionary<int, List<GameObject>>(); // Cache for floor platforms
    private List<GameObject> currentPlatforms = new List<GameObject>(); // Currently active platforms
    private float currentTime;                            // Current remaining time
    private bool gameRunning;                             // Flag indicating if game is running
    private int currentFloor = 0;                         // Current floor level (0=easy, 1=medium, 2=hard)
    public Vector3 checkpointPosition;                    // Player's respawn position
    private List<string> safeColors = new List<string>(); // List of safe colors for current puzzle
    private bool isPlayerFalling = false;                 // Flag to prevent multiple fall events
    public PlatFormState currentState;

    // Singleton initialization
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

    // Initial setup when game starts
    private void Start()
    {
        // Show the start screen
        // ShowStartScreen();
        
        // Get reference to mesh collider
        meshCollider = GetComponent<MeshCollider>();

    }
    void Update()
    {
        if (gameRunning)
        {
            // Update timer
            currentTime -= Time.deltaTime;

            // Check for game over condition
            if (currentTime <= 0)
            {
                GameOver();
            }
        }
        switch (currentState)
        {
            case PlatFormState.TF1:
                //if {gameObject.CompareTag == "Green"} ;
                TF1();
                break;
            case PlatFormState.TF2:
                TF2();
                break;
            case PlatFormState.TF3:
                TF3();
                break;
            case PlatFormState.TF4:
                TF4();
                break;
            case PlatFormState.TF5:
                TF5();
                break;
            case PlatFormState.TF6:
                TF6();
                break;
            case PlatFormState.TF7:
                TF7();
                break;
            case PlatFormState.TF8:
                TF8();
                break;
            case PlatFormState.TF9:
                TF9();
                break;
            case PlatFormState.TF10:
                TF10();
                break;

        }
    }
    //public void OnCollisionEnter(Collision collision)
    //{
        // Check if the collision is with the player
        //if (collision.gameObject.CompareTag("Player"))
        //{
            // Check which color platform was touched
            //if (gameObject.CompareTag("Red"))
            //{
                //FindObjectOfType<ColliderDisable>().DisablePlatform();
                //Debug.Log("Red platform disabled");
            //}
            //else if (gameObject.CompareTag("Yellow"))
            //{
               // FindObjectOfType<ColliderDisable>().DisablePlatform();
                //Debug.Log("Yellow platform disabled");
            //}
            //else if (gameObject.CompareTag("Green"))
            //{
                //FindObjectOfType<ColliderDisable>().DisablePlatform();
                //Debug.Log("Green platform disabled");
            //}
        //}
    //}


    public void TF1()
    {
        /*make red be the only one correct*/
        //gameObject.CompareTag("Yellow");
        //{
            //FindObjectOfType<ColliderDisable>().DisablePlatform();
            //Debug.Log("Yellow platform disabled");
        //}
        //gameObject.CompareTag("Green");
        //{
            //FindObjectOfType<ColliderDisable>().DisablePlatform();
            //Debug.Log("Green platform disabled");
        //}

    }
    public void TF2() 
    {
        /* ... */
        
    }
    public void TF3() { /* ... */ }
    public void TF4() { /* ... */ }
    public void TF5() { /* ... */ }
    public void TF6() { /* ... */ }
    public void TF7() { /* ... */ }
    public void TF8() { /* ... */ }
    public void TF9() { /* ... */ }
    public void TF10() { /* ... */ }


    // Main game loop


    // Starts the game
    public void StartGame()
    {
        // Initialize game state
        currentFloor = 0;
        currentTime = levelTime;
        gameRunning = true;

        // Set up first floor
        SetupFloor(currentFloor);

        // Position player at spawn point
        player.Respawn(floorSpawnPoints[currentFloor].position);

        // Set checkpoint to spawn point
        checkpointPosition = floorSpawnPoints[currentFloor].position;
    }

    // Sets up a specific floor level
    public void SetupFloor(int floorLevel)
    {
        // Get platforms for this floor from cache
        currentPlatforms = new List<GameObject>(floorPlatformsCache[floorLevel]);

        // Generate puzzle rules for this floor
        GeneratePuzzleRules();

        // Process all platforms on this floor
        ProcessPlatforms();
    }

    // Processes all platforms on current floor
    public void ProcessPlatforms()
    {
        foreach (var platform in currentPlatforms)
        {
            // Try to get MeshCollider first, then fall back to any Collider
            MeshCollider meshCollider = platform.GetComponent<MeshCollider>();
            Collider anyCollider = meshCollider != null ? meshCollider : platform.GetComponent<Collider>();

            // Enable collider if it exists
            if (anyCollider != null)
            {
                anyCollider.enabled = true;
            }

            // Get or add platform state component
            PlatformState state = platform.GetComponent<PlatformState>() ?? platform.AddComponent<PlatformState>();
            state.isDisabled = false;
            state.isSafe = IsPlatformSafe(platform);
        }
    }

    // Checks if a platform is safe to land on
    private bool IsPlatformSafe(GameObject platform)
    {
        // End platform is always safe
        // if (platform.CompareTag()) return true;

        // Check if platform color is in safe colors list
        string platformColor = GetPlatformColor(platform);
        return safeColors.Contains(platformColor);
    }

    // Called when player lands on a platform
    public void OnPlatformLanded(GameObject platform)
    {
        PlatformState state = platform.GetComponent<PlatformState>();
        string platformColor = GetPlatformColor(platform);

        // Check if platform should be disabled
        if (ShouldDisablePlatform(platformColor, state))
        {
            DisablePlatformCollider(platform); // Disable collider immediately
            PlayerFell(); // Trigger fall sequence
            return;
        }
    }

    // Disables a platform's collider
    private void DisablePlatformCollider(GameObject platform)
    {
        // Try MeshCollider first, then fall back to any Collider
        MeshCollider meshCollider = platform.GetComponent<MeshCollider>();
        Collider anyCollider = meshCollider != null ? meshCollider : platform.GetComponent<Collider>();

        // Disable the collider if found
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

    // Determines if a platform should be disabled
    public bool ShouldDisablePlatform(string platformColor, PlatformState state)
    {
        return (state != null && !state.isSafe && !state.isDisabled &&
               !safeColors.Contains(platformColor));
    }

    // Handles player falling off platform
    public void PlayerFell()
    {
        if (isPlayerFalling) return; // Prevent multiple fall events

        isPlayerFalling = true;

        // Deduct time penalty based on floor level
        currentTime -= Mathf.Max(2, 7 - currentFloor * 2);

        // Start respawn sequence
        StartCoroutine(RespawnPlayerAfterDelay(1f));
    }

    // Coroutine to respawn player after delay
    private IEnumerator RespawnPlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        player.Respawn(checkpointPosition);
        isPlayerFalling = false;
    }

    // Advances player to next floor
    private void AdvanceToNextFloor(int newFloor)
    {
        currentFloor = newFloor;
        currentTime = levelTime;
        checkpointPosition = floorSpawnPoints[currentFloor].position;

        // Set up new floor
        SetupFloor(currentFloor);

        // Respawn player at new floor's spawn point
        player.Respawn(checkpointPosition);
    }

    // Generates puzzle rules for current floor
    private void GeneratePuzzleRules()
    {
        safeColors.Clear();

        // Determine number of safe colors (1 or 2)
        int safeCount = Random.Range(1, 3);
        List<string> allColors = new List<string> { "GREEN", "RED", "YELLOW" };

        // Randomly select safe colors
        for (int i = 0; i < safeCount; i++)
        {
            int randomIndex = Random.Range(0, allColors.Count);
            safeColors.Add(allColors[randomIndex]);
            allColors.RemoveAt(randomIndex);
        }

        // Generate statements (one true, others false)
        int trueIndex = Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            bool isTrue = (i == trueIndex);
            statementTexts[i].text = GenerateStatement(isTrue);
            statementTexts[i].color = isTrue ? Color.green : Color.red;
        }
    }

    // Generates a puzzle statement (true or false)
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
            // Generate false statement
            string falseStatement;
            int attempts = 0;

            // Keep trying until we get a valid false statement
            do
            {
                attempts++;
                if (attempts > 100)
                {
                    return $"You CAN touch {GetRandomColorExcept(safeColors)}";
                }

                // Randomly select statement type
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

    // Checks if a statement is actually true (to avoid false statements that are true)
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

    // Gets the color of a platform
    private string GetPlatformColor(GameObject platform)
    {
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color platformColor = renderer.material.color;

            // Compare platform color to known colors
            if (IsColorSimilar(platformColor, Color.green)) return "GREEN";
            if (IsColorSimilar(platformColor, Color.red)) return "RED";
            if (IsColorSimilar(platformColor, Color.yellow)) return "YELLOW";
        }
        return "GREEN"; // Default color
    }

    // Compares two colors for similarity
    private bool IsColorSimilar(Color a, Color b)
    {
        return Vector4.Distance(a, b) < colorSimilarityThreshold;
    }

    // Updates the timer display
    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Gets a random color
    private string GetRandomColor()
    {
        string[] colors = { "GREEN", "RED", "YELLOW" };
        return colors[Random.Range(0, colors.Length)];
    }

    // Gets a random color excluding specified colors
    private string GetRandomColorExcept(List<string> excludedColors)
    {
        List<string> available = new List<string> { "GREEN", "RED", "YELLOW" };
        foreach (var color in excludedColors)
        {
            available.Remove(color);
        }
        return available[Random.Range(0, available.Count)];
    }

    // Shows the start screen
    //private void ShowStartScreen()
    //{
    //SceneManager.LoadScene("Start");
    //}

    // Handles game over
    private void GameOver()
    {
        SceneManager.LoadScene("Restart");
    }

    // Handles game win
    private void GameWin()
    {
        SceneManager.LoadScene("Win");
    }

    // Restarts the game
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Refreshes puzzle statements (currently commented out)
    public void RefreshStatements()
    {
        GeneratePuzzleRules();
    }

    // Triggers win condition
    public void WinTrigger()
    {
        GameWin();
    }
}
