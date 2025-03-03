using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides UI controls for testing champion movement and lane functionality.
/// </summary>
public class TestUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the test scene setup")]
    public TestSceneSetup testSceneSetup;
    
    [Header("UI Elements")]
    [Tooltip("Button to spawn test entities")]
    public Button spawnButton;
    
    [Tooltip("Button to clear test entities")]
    public Button clearButton;
    
    [Tooltip("Button to start movement")]
    public Button startMovementButton;
    
    [Tooltip("Button to stop movement")]
    public Button stopMovementButton;
    
    [Tooltip("Button to disengage all champions")]
    public Button disengageButton;
    
    [Tooltip("Dropdown to select lane type")]
    public Dropdown laneTypeDropdown;
    
    [Tooltip("Dropdown to select team")]
    public Dropdown teamDropdown;
    
    private void Start()
    {
        // Find test scene setup if not assigned
        if (testSceneSetup == null)
        {
            testSceneSetup = FindObjectOfType<TestSceneSetup>();
        }
        
        // Set up button listeners
        SetupButtonListeners();
        
        // Set up dropdowns
        SetupDropdowns();
    }
    
    /// <summary>
    /// Sets up button listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(SpawnTestEntities);
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearTestEntities);
        }
        
        if (startMovementButton != null)
        {
            startMovementButton.onClick.AddListener(StartMovement);
        }
        
        if (stopMovementButton != null)
        {
            stopMovementButton.onClick.AddListener(StopMovement);
        }
        
        if (disengageButton != null)
        {
            disengageButton.onClick.AddListener(DisengageAllChampions);
        }
    }
    
    /// <summary>
    /// Sets up dropdown options
    /// </summary>
    private void SetupDropdowns()
    {
        // Set up lane type dropdown
        if (laneTypeDropdown != null)
        {
            laneTypeDropdown.ClearOptions();
            laneTypeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Top Lane",
                "Mid Lane",
                "Bottom Lane"
            });
        }
        
        // Set up team dropdown
        if (teamDropdown != null)
        {
            teamDropdown.ClearOptions();
            teamDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Blue Team",
                "Red Team"
            });
        }
    }
    
    /// <summary>
    /// Spawns test entities
    /// </summary>
    public void SpawnTestEntities()
    {
        if (testSceneSetup != null)
        {
            testSceneSetup.SpawnAllChampions();
            
            // Set up newly spawned entities with selected options
            SetupSpawnedEntities();
        }
        else
        {
            Debug.LogError("Test scene setup not found!");
        }
    }
    
    /// <summary>
    /// Clears test entities
    /// </summary>
    public void ClearTestEntities()
    {
        if (testSceneSetup != null)
        {
            testSceneSetup.ClearTestEntities();
        }
        else
        {
            Debug.LogError("Test scene setup not found!");
        }
    }
    
    /// <summary>
    /// Starts movement for all test entities
    /// </summary>
    public void StartMovement()
    {
        TestEntity[] testEntities = FindObjectsOfType<TestEntity>();
        foreach (TestEntity entity in testEntities)
        {
            entity.StartMoving();
        }
        
        Debug.Log($"Started movement for {testEntities.Length} test entities");
    }
    
    /// <summary>
    /// Stops movement for all test entities
    /// </summary>
    public void StopMovement()
    {
        TestEntity[] testEntities = FindObjectsOfType<TestEntity>();
        foreach (TestEntity entity in testEntities)
        {
            entity.StopMoving();
        }
        
        Debug.Log($"Stopped movement for {testEntities.Length} test entities");
    }
    
    /// <summary>
    /// Sets up spawned entities with selected options
    /// </summary>
    private void SetupSpawnedEntities()
    {
        // Get selected lane type
        LaneType selectedLaneType = LaneType.Bottom;
        if (laneTypeDropdown != null)
        {
            selectedLaneType = (LaneType)laneTypeDropdown.value;
        }
        
        // Get selected team
        Team selectedTeam = Team.Blue;
        if (teamDropdown != null)
        {
            selectedTeam = (Team)teamDropdown.value;
        }
        
        // Find all test entities and set their properties
        TestEntity[] testEntities = FindObjectsOfType<TestEntity>();
        foreach (TestEntity entity in testEntities)
        {
            // Set team and lane type
            if (entity.champion != null)
            {
                entity.champion.SetTeam(selectedTeam);
                entity.champion.SetLaneType(selectedLaneType);
            }
            
            // Update entity properties
            entity.team = selectedTeam;
            entity.laneType = selectedLaneType;
            
            // Refresh entity setup
            entity.SendMessage("SetupEntity", null, SendMessageOptions.DontRequireReceiver);
        }
        
        Debug.Log($"Set up {testEntities.Length} test entities with lane type {selectedLaneType} and team {selectedTeam}");
    }
    
    /// <summary>
    /// Disengages all champions from their opponents
    /// </summary>
    public void DisengageAllChampions()
    {
        ChampionMovement[] champions = FindObjectsOfType<ChampionMovement>();
        
        foreach (ChampionMovement champion in champions)
        {
            champion.DisengageFromOpponent();
        }
        
        Debug.Log($"Disengaged {champions.Length} champions from their opponents");
    }
} 