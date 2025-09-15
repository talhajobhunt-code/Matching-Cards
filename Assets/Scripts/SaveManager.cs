using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    [Header("Save Settings")]
    public string saveFileName = "cardgame_save.json";
    public bool autoSave = true;
    public float autoSaveInterval = 30f;

    // Dependencies
    private GameManager gameManager;
    private string saveFilePath;
    private float lastSaveTime;

    // Properties
    public bool HasSaveFile => File.Exists(saveFilePath);
    public string SaveFilePath => saveFilePath;

    public void Initialize(GameManager gm)
    {
        gameManager = gm;
        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);

        Debug.Log($"Save file path: {saveFilePath}");
    }

    private void Update()
    {
        if (autoSave && gameManager != null && gameManager.CurrentState == GameState.Playing)
        {
            if (Time.time - lastSaveTime >= autoSaveInterval)
            {
                SaveGame();
            }
        }
    }

    public bool SaveGame()
    {
        if (gameManager == null)
        {
            Debug.LogError("Cannot save: GameManager is null");
            return false;
        }

        try
        {
            GameSaveData saveData = CreateSaveData();
            string jsonData = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(saveFilePath, jsonData);

            lastSaveTime = Time.time;
            Debug.Log($"Game saved successfully to: {saveFilePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
            return false;
        }
    }

    public bool LoadGame()
    {
        if (!HasSaveFile)
        {
            Debug.LogWarning("No save file found");
            return false;
        }

        if (gameManager == null)
        {
            Debug.LogError("Cannot load: GameManager is null");
            return false;
        }

        try
        {
            string jsonData = File.ReadAllText(saveFilePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);

            if (saveData == null)
            {
                Debug.LogError("Save data is null after loading");
                return false;
            }

            ApplySaveData(saveData);
            Debug.Log("Game loaded successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return false;
        }
    }

    private GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData();

        // Basic game info
        saveData.gameVersion = Application.version;
        saveData.saveTime = System.DateTime.Now.ToBinary();
        saveData.gameState = gameManager.CurrentState;

        // Board configuration
        saveData.boardWidth = gameManager.boardWidth;
        saveData.boardHeight = gameManager.boardHeight;

        // Score data
        var scoreManager = gameManager.GetComponent<ScoreManager>();
        if (scoreManager != null)
        {
            saveData.scoreData = scoreManager.GetScoreData();
        }

        // Card data
        var cardManager = gameManager.GetComponent<CardManager>();
        if (cardManager != null)
        {
            saveData.cardData = cardManager.GetSaveData();
        }

        return saveData;
    }

    private void ApplySaveData(GameSaveData saveData)
    {
        // Verify save data version compatibility
        if (!IsCompatibleVersion(saveData.gameVersion))
        {
            Debug.LogWarning($"Save file version ({saveData.gameVersion}) may not be compatible with current version ({Application.version})");
        }

        // Apply board configuration
        gameManager.boardWidth = saveData.boardWidth;
        gameManager.boardHeight = saveData.boardHeight;

        // Recreate the board with the same configuration
        gameManager.StartNewGame();

        // Apply score data
        var scoreManager = gameManager.GetComponent<ScoreManager>();
        if (scoreManager != null && saveData.scoreData != null)
        {
            scoreManager.LoadScoreData(saveData.scoreData);
        }

        // Apply card states
        var cardManager = gameManager.GetComponent<CardManager>();
        if (cardManager != null && saveData.cardData != null)
        {
            cardManager.LoadFromSaveData(saveData.cardData);
        }

        // Set game state
        gameManager.ChangeGameState(saveData.gameState);
    }

    public bool DeleteSaveFile()
    {
        if (!HasSaveFile)
        {
            Debug.LogWarning("No save file to delete");
            return false;
        }

        try
        {
            File.Delete(saveFilePath);
            Debug.Log("Save file deleted successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
            return false;
        }
    }

    private bool IsCompatibleVersion(string saveVersion)
    {
        // Simple version compatibility check
        // You can implement more sophisticated version checking here
        return string.IsNullOrEmpty(saveVersion) || saveVersion == Application.version;
    }

    public GameSaveData GetSaveData()
    {
        if (!HasSaveFile) return null;

        try
        {
            string jsonData = File.ReadAllText(saveFilePath);
            return JsonUtility.FromJson<GameSaveData>(jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to read save data: {e.Message}");
            return null;
        }
    }

    public string GetSaveFileInfo()
    {
        if (!HasSaveFile) return "No save file found";

        try
        {
            FileInfo fileInfo = new FileInfo(saveFilePath);
            GameSaveData saveData = GetSaveData();

            if (saveData != null)
            {
                System.DateTime saveTime = System.DateTime.FromBinary(saveData.saveTime);
                return $"Save File Info:\n" +
                       $"  Last Modified: {fileInfo.LastWriteTime}\n" +
                       $"  Save Time: {saveTime}\n" +
                       $"  Board Size: {saveData.boardWidth}x{saveData.boardHeight}\n" +
                       $"  Game State: {saveData.gameState}\n" +
                       $"  File Size: {fileInfo.Length} bytes";
            }
            else
            {
                return $"Save file exists but cannot be read\n" +
                       $"  Last Modified: {fileInfo.LastWriteTime}\n" +
                       $"  File Size: {fileInfo.Length} bytes";
            }
        }
        catch (System.Exception e)
        {
            return $"Error reading save file info: {e.Message}";
        }
    }

    // Quick save/load methods
    public void QuickSave()
    {
        if (gameManager.CurrentState == GameState.Playing)
        {
            SaveGame();
        }
    }

    public void QuickLoad()
    {
        LoadGame();
    }

    // Auto-save controls
    public void EnableAutoSave(bool enable)
    {
        autoSave = enable;
    }

    public void SetAutoSaveInterval(float interval)
    {
        autoSaveInterval = Mathf.Max(5f, interval); // Minimum 5 seconds
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && gameManager != null && gameManager.CurrentState == GameState.Playing)
        {
            SaveGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && gameManager != null && gameManager.CurrentState == GameState.Playing)
        {
            SaveGame();
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null && gameManager.CurrentState == GameState.Playing)
        {
            SaveGame();
        }
    }
}

[System.Serializable]
public class GameSaveData
{
    public string gameVersion;
    public long saveTime;
    public GameState gameState;
    public int boardWidth;
    public int boardHeight;
    public ScoreData scoreData;
    public CardSaveData cardData;
}