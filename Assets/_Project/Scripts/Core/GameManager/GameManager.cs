using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Game state
    public enum GameState
    {
        MainMenu,
        PreMatch,
        InMatch,
        PostMatch
    }

    public GameState CurrentGameState { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeGameState(GameState newState)
    {
        CurrentGameState = newState;
        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenuState();
                break;
            case GameState.PreMatch:
                HandlePreMatchState();
                break;
            case GameState.InMatch:
                HandleInMatchState();
                break;
            case GameState.PostMatch:
                HandlePostMatchState();
                break;
        }
    }

    private void HandleMainMenuState()
    {
        Debug.Log("Entering Main Menu State");
    }

    private void HandlePreMatchState()
    {
        Debug.Log("Entering Pre-Match State");
    }

    private void HandleInMatchState()
    {
        Debug.Log("Entering Match State");
    }

    private void HandlePostMatchState()
    {
        Debug.Log("Entering Post-Match State");
    }
} 