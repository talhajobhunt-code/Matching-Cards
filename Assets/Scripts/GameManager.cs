using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int boardWidth = 4;
    public int boardHeight = 4;
    public float cardSpacing = 1.1f;
    public Transform cardParent;

    [Header("Prefabs")]
    public GameObject cardPrefab;

    [Header("Card Images")]
    public Sprite[] cardImages;
    public Sprite cardBackImage;

    // Events
    public event Action<int> OnScoreChanged;
    public event Action<GameState> OnGameStateChanged;
    public event Action OnGameWon;

    // Components
    private CardManager cardManager;
    private ScoreManager scoreManager;
    private AudioManager audioManager;
    private SaveManager saveManager;
    private BoardManager boardManager;

    // Game State
    private GameState currentState;
    private List<Card> allCards = new List<Card>();

    public GameState CurrentState => currentState;
    public List<Card> AllCards => allCards;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        StartNewGame();
    }

    private void InitializeComponents()
    {
        cardManager = GetComponent<CardManager>() ?? gameObject.AddComponent<CardManager>();
        scoreManager = GetComponent<ScoreManager>() ?? gameObject.AddComponent<ScoreManager>();
        audioManager = FindObjectOfType<AudioManager>();
        saveManager = GetComponent<SaveManager>() ?? gameObject.AddComponent<SaveManager>();
        boardManager = GetComponent<BoardManager>() ?? gameObject.AddComponent<BoardManager>();

        // Initialize components with dependencies
        cardManager.Initialize(this, audioManager);
        scoreManager.Initialize(this);
        boardManager.Initialize(this);
        saveManager.Initialize(this);

        // Subscribe to events
        cardManager.OnCardMatched += OnCardMatched;
        cardManager.OnCardMismatched += OnCardMismatched;
        cardManager.OnAllCardsMatched += OnAllCardsMatched;
    }

    public void StartNewGame()
    {
        ChangeGameState(GameState.Playing);
        scoreManager.ResetScore();
        CreateGameBoard();
    }

    public void LoadGame()
    {
        if (saveManager.LoadGame())
        {
            ChangeGameState(GameState.Playing);
        }
        else
        {
            StartNewGame();
        }
    }

    private void CreateGameBoard()
    {
        ClearBoard();
        allCards = boardManager.CreateBoard(boardWidth, boardHeight, cardImages, cardBackImage, cardPrefab, cardParent, cardSpacing);
        cardManager.SetCards(allCards);
    }

    private void ClearBoard()
    {
        foreach (Transform child in cardParent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        allCards.Clear();
    }

    public void OnPause(bool state)
    {
        foreach (Transform child in cardParent)
        {
          child.gameObject.SetActive(state);
        }
    }

    public void ChangeGameState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
    }

    private void OnCardMatched(Card card1, Card card2)
    {
        scoreManager.AddMatchScore();
        saveManager.SaveGame();
    }

    private void OnCardMismatched(Card card1, Card card2)
    {
        scoreManager.ResetCombo();
    }

    private void OnAllCardsMatched()
    {
        ChangeGameState(GameState.GameWon);
        OnGameWon?.Invoke();
        audioManager?.PlayGameOverSound();
    }

    public void RestartGame()
    {
        StartNewGame();
    }

    public void PauseGame()
    {
        ChangeGameState(GameState.Paused);
        OnPause(false);
    }

    public void ResumeGame()
    {
        ChangeGameState(GameState.Playing);
        OnPause(true);
    }

    private void OnDestroy()
    {
        if (cardManager != null)
        {
            cardManager.OnCardMatched -= OnCardMatched;
            cardManager.OnCardMismatched -= OnCardMismatched;
            cardManager.OnAllCardsMatched -= OnAllCardsMatched;
        }
    }
}

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameWon,
    GameOver
}