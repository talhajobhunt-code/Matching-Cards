using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [Header("Camera Settings for Card Game")]
    public bool autoSetup = true;
    public float orthographicSize = 6f;
    public Vector3 cameraPosition = new Vector3(0, 0, -10);

    [Header("Card Visibility Settings")]
    public LayerMask cardLayerMask = -1;
    public float zPosition = 0f; // Z position for cards

    private Camera gameCamera;
    private GameManager gameManager;

    private void Awake()
    {
        gameCamera = GetComponent<Camera>();
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        gameManager = FindObjectOfType<GameManager>();

        if (autoSetup)
        {
            SetupCameraFor2D();
        }
    }

    private void Start()
    {
        if (autoSetup)
        {
            AdjustCameraToFitBoard();
        }
    }

    public void SetupCameraFor2D()
    {
        if (gameCamera == null) return;

        // Configure camera for 2D sprites
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = orthographicSize;
        gameCamera.transform.position = cameraPosition;

        // Set camera culling mask to see cards
        gameCamera.cullingMask = cardLayerMask;

        // Clear settings for 2D
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = Color.black;

        Debug.Log($"Camera setup for 2D: Position={cameraPosition}, Size={orthographicSize}");
    }

    public void AdjustCameraToFitBoard()
    {
        if (gameManager == null || gameCamera == null) return;

        // Calculate required camera size to fit the board
        BoardManager boardManager = gameManager.GetComponent<BoardManager>();
        if (boardManager != null)
        {
            Vector2 boardBounds = boardManager.GetBoardBounds(
                gameManager.boardWidth,
                gameManager.boardHeight,
                Vector2.one, // We'll calculate this properly
                gameManager.cardSpacing
            );

            // Add some padding
            float padding = 2f;
            float requiredSize = Mathf.Max(boardBounds.x, boardBounds.y) * 0.5f + padding;

            gameCamera.orthographicSize = requiredSize;

            Debug.Log($"Adjusted camera size to {requiredSize} for board bounds {boardBounds}");
        }
    }

    [ContextMenu("Setup Camera for 2D")]
    public void ForceSetupCamera()
    {
        SetupCameraFor2D();
        AdjustCameraToFitBoard();
    }

    [ContextMenu("Debug Camera and Card Positions")]
    public void DebugCameraAndCards()
    {
        Debug.Log("=== CAMERA & CARD DEBUG ===");

        if (gameCamera != null)
        {
            Debug.Log($"Camera Position: {gameCamera.transform.position}");
            Debug.Log($"Camera Orthographic: {gameCamera.orthographic}");
            Debug.Log($"Camera Orthographic Size: {gameCamera.orthographicSize}");
            Debug.Log($"Camera Culling Mask: {gameCamera.cullingMask}");
        }

        if (gameManager?.cardParent != null)
        {
            Debug.Log($"Card Parent Position: {gameManager.cardParent.position}");
            Debug.Log($"Card Parent Child Count: {gameManager.cardParent.childCount}");

            // Check first few cards
            for (int i = 0; i < Mathf.Min(3, gameManager.cardParent.childCount); i++)
            {
                Transform card = gameManager.cardParent.GetChild(i);
                SpriteRenderer sr = card.GetComponent<SpriteRenderer>();

                Debug.Log($"Card {i}:");
                Debug.Log($"  Position: {card.position}");
                Debug.Log($"  Local Position: {card.localPosition}");
                Debug.Log($"  Scale: {card.localScale}");
                Debug.Log($"  Has SpriteRenderer: {sr != null}");
                if (sr != null)
                {
                    Debug.Log($"  Sprite: {sr.sprite?.name}");
                    Debug.Log($"  Color: {sr.color}");
                    Debug.Log($"  Sorting Layer: {sr.sortingLayerName}");
                    Debug.Log($"  Order in Layer: {sr}");
                }
            }
        }

        Debug.Log("===========================");
    }

    // Method to ensure cards are visible
    public void EnsureCardsVisible()
    {
        if (gameManager?.cardParent == null) return;

        foreach (Transform cardTransform in gameManager.cardParent)
        {
            // Ensure cards are at correct Z position
            Vector3 pos = cardTransform.position;
            pos.z = zPosition;
            cardTransform.position = pos;

            // Ensure sprite renderer is properly configured
            SpriteRenderer sr = cardTransform.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 0;
                sr.color = Color.white;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (gameCamera == null) return;

        // Draw camera frustum
        Gizmos.color = Color.yellow;

        float height = gameCamera.orthographicSize * 2;
        float width = height * gameCamera.aspect;

        Vector3 center = gameCamera.transform.position;
        center.z = 0;

        Gizmos.DrawWireCube(center, new Vector3(width, height, 0.1f));

        // Draw card parent position
        if (gameManager?.cardParent != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(gameManager.cardParent.position, 0.5f);
        }
    }
}