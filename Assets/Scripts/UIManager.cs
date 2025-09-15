using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject gameplayPanel;
    public GameObject menuPanel;
    public GameObject gameWonPanel;
    public GameObject pausePanel;

    [Header("Gameplay UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI gameTimeText;
    public Button pauseButton;
    public Button restartButton;

    [Header("Menu UI")]
    public Button newGameButton;
    public Button loadGameButton;
    public Slider boardWidthSlider;
    public Slider boardHeightSlider;
    public TextMeshProUGUI boardSizeText;

    [Header("Game Won UI")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalStatsText;
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Pause UI")]
    public Button resumeButton;
    public Button pauseToMenuButton;

    [Header("Animation Settings")]
    public float panelTransitionDuration = 0.3f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Dependencies
    private GameManager gameManager;
    private ScoreManager scoreManager;
    private SaveManager saveManager;

    // UI State
    private GameObject currentPanel;
    private Coroutine gameTimeCoroutine;
    private void Awake()
    {
        InitializeUI();
    }

    private void Start()
    {
        FindDependencies();
        SetupEventListeners();
        ShowPanel(menuPanel);
    }

    private void InitializeUI()
    {
        // Initialize board size sliders
        if (boardWidthSlider != null)
        {
            boardWidthSlider.minValue = 2;
            boardWidthSlider.maxValue = 8;
            boardWidthSlider.value = 4;
            boardWidthSlider.wholeNumbers = true;
        }

        if (boardHeightSlider != null)
        {
            boardHeightSlider.minValue = 2;
            boardHeightSlider.maxValue = 8;
            boardHeightSlider.value = 4;
            boardHeightSlider.wholeNumbers = true;
        }

        UpdateBoardSizeText();
    }

    private void FindDependencies()
    {
        gameManager = FindObjectOfType<GameManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        saveManager = FindObjectOfType<SaveManager>();
    }

    private void SetupEventListeners()
    {
        // Gameplay buttons
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        // Menu buttons
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);
        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(OnLoadGameClicked);

        // Board size sliders
        if (boardWidthSlider != null)
            boardWidthSlider.onValueChanged.AddListener(OnBoardSizeChanged);
        if (boardHeightSlider != null)
            boardHeightSlider.onValueChanged.AddListener(OnBoardSizeChanged);

        // Game Won buttons
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Pause buttons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseToMenuButton != null)
            pauseToMenuButton.onClick.AddListener(OnPauseToMenuClicked);

        // Subscribe to game events
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += OnGameStateChanged;
            gameManager.OnGameWon += OnGameWon;
        }

        if (scoreManager != null)
        {
            scoreManager.OnScoreUpdated += OnScoreUpdated;
        }

        // Update load button availability
        if (loadGameButton != null && saveManager != null)
        {
            loadGameButton.interactable = saveManager.HasSaveFile;
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Menu:
                ShowPanel(menuPanel);
                StopGameTimeUpdate();
                break;
            case GameState.Playing:
                ShowPanel(gameplayPanel);
                StartGameTimeUpdate();
                break;
            case GameState.Paused:
                ShowPanel(pausePanel);
                StopGameTimeUpdate();
                break;
            case GameState.GameWon:
                ShowPanel(gameWonPanel);
                StopGameTimeUpdate();
                UpdateFinalScoreUI();
                break;
        }
    }

    private void OnScoreUpdated(ScoreData scoreData)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {scoreData.score:N0}";

        if (comboText != null)
        {
            if (scoreData.combo > 1)
            {
                comboText.text = $"Combo x{scoreData.combo}!";
                comboText.gameObject.SetActive(true);
                StartCoroutine(AnimateComboText());
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        if (statsText != null)
        {
            float accuracy = scoreData.accuracy * 100f;
            statsText.text = $"Matches: {scoreData.matches} | Accuracy: {accuracy:F1}%";
        }
    }

    private IEnumerator AnimateComboText()
    {
        if (comboText == null) yield break;

        Vector3 originalScale = comboText.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        // Scale up
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            comboText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Scale down
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            comboText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        comboText.transform.localScale = originalScale;
    }

    private void StartGameTimeUpdate()
    {
        if (gameTimeCoroutine != null)
        {
            StopCoroutine(gameTimeCoroutine);
        }
        gameTimeCoroutine = StartCoroutine(UpdateGameTimeUI());
    }

    private void StopGameTimeUpdate()
    {
        if (gameTimeCoroutine != null)
        {
            StopCoroutine(gameTimeCoroutine);
            gameTimeCoroutine = null;
        }
    }

    private IEnumerator UpdateGameTimeUI()
    {
        while (gameTimeText != null && scoreManager != null)
        {
            float gameTime = scoreManager.GameTime;
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);

            gameTimeText.text = $"Time: {minutes:00}:{seconds:00}";

            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateFinalScoreUI()
    {
        if (scoreManager == null) return;

        int finalScore = scoreManager.CalculateFinalScore();
        ScoreData scoreData = scoreManager.GetScoreData();

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore:N0}";
        }

        if (finalStatsText != null)
        {
            int minutes = Mathf.FloorToInt(scoreData.gameTime / 60f);
            int seconds = Mathf.FloorToInt(scoreData.gameTime % 60f);
            float accuracy = scoreData.accuracy * 100f;

            finalStatsText.text = $"Time: {minutes:00}:{seconds:00}\n" +
                                $"Matches: {scoreData.matches}\n" +
                                $"Accuracy: {accuracy:F1}%\n" +
                                $"Best Combo: {scoreData.combo}";
        }
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == currentPanel) return;

        StartCoroutine(TransitionToPanel(panel));
    }

    private IEnumerator TransitionToPanel(GameObject newPanel)
    {
        // Fade out current panel
        if (currentPanel != null)
        {
            yield return StartCoroutine(FadePanel(currentPanel, 1f, 0f));
            currentPanel.SetActive(false);
        }

        // Fade in new panel
        if (newPanel != null)
        {
            newPanel.SetActive(true);
            currentPanel = newPanel;
            yield return StartCoroutine(FadePanel(newPanel, 0f, 1f));
        }
    }

    private IEnumerator FadePanel(GameObject panel, float fromAlpha, float toAlpha)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;

        while (elapsed < panelTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / panelTransitionDuration;
            t = transitionCurve.Evaluate(t);

            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }

    private void OnBoardSizeChanged(float value)
    {
        UpdateBoardSizeText();
    }

    private void UpdateBoardSizeText()
    {
        if (boardSizeText != null && boardWidthSlider != null && boardHeightSlider != null)
        {
            int width = Mathf.RoundToInt(boardWidthSlider.value);
            int height = Mathf.RoundToInt(boardHeightSlider.value);
            int totalCards = width * height;

            // Ensure even number of cards
            if (totalCards % 2 != 0)
            {
                totalCards--;
                // Don't modify sliders here to avoid infinite loop
            }

            int pairs = totalCards / 2;
            boardSizeText.text = $"Board Size: {width} x {height}\n({totalCards} cards, {pairs} pairs)";

            // Enable/disable load button based on save availability
            if (loadGameButton != null && saveManager != null)
            {
                loadGameButton.interactable = saveManager.HasSaveFile;
            }
        }
    }

    // Button Event Handlers
    private void OnNewGameClicked()
    {
        if (gameManager != null)
        {
            // Apply board size settings
            if (boardWidthSlider != null && boardHeightSlider != null)
            {
                gameManager.boardWidth = Mathf.RoundToInt(boardWidthSlider.value);
                gameManager.boardHeight = Mathf.RoundToInt(boardHeightSlider.value);
            }

            gameManager.StartNewGame();
        }
    }

    private void OnLoadGameClicked()
    {
        if (gameManager != null)
        {
            gameManager.LoadGame();
        }
    }

    private void OnPauseClicked()
    {
        if (gameManager != null)
        {
            gameManager.PauseGame();
        }
    }

    private void OnResumeClicked()
    {
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
    }

    private void OnRestartClicked()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    private void OnPlayAgainClicked()
    {
        if (gameManager != null)
        {
            gameManager.StartNewGame();
        }
    }

    private void OnMainMenuClicked()
    {
        if (gameManager != null)
        {
            gameManager.ChangeGameState(GameState.Menu);
            gameManager.OnPause(false);
        }
    }

    private void OnPauseToMenuClicked()
    {
        if (gameManager != null)
        {
            gameManager.OnPause(false);
            gameManager.ChangeGameState(GameState.Menu);

        }
    }

    private void OnGameWon()
    {
        StartCoroutine(ShowWinAnimation());
        gameManager.OnPause(false);
    }

    private IEnumerator ShowWinAnimation()
    {
        // Add a celebration animation here
        if (finalScoreText != null)
        {
            Vector3 originalScale = finalScoreText.transform.localScale;

            // Bounce animation
            for (int i = 0; i < 3; i++)
            {
                finalScoreText.transform.localScale = originalScale * 1.1f;
                yield return new WaitForSeconds(0.1f);
                finalScoreText.transform.localScale = originalScale;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // Input handling
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // ESC key handling
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameManager != null)
            {
                switch (gameManager.CurrentState)
                {
                    case GameState.Playing:
                        OnPauseClicked();
                        break;
                    case GameState.Paused:
                        OnResumeClicked();
                        break;
                    case GameState.GameWon:
                        OnMainMenuClicked();
                        break;
                }
            }
        }

        // Quick restart with R key
        if (Input.GetKeyDown(KeyCode.R) && gameManager != null && gameManager.CurrentState == GameState.Playing)
        {
            OnRestartClicked();
        }
    }

    private void OnDestroy()
    {
        // Cleanup event subscriptions
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= OnGameStateChanged;
            gameManager.OnGameWon -= OnGameWon;
        }

        if (scoreManager != null)
        {
            scoreManager.OnScoreUpdated -= OnScoreUpdated;
        }

        StopGameTimeUpdate();
    }
}