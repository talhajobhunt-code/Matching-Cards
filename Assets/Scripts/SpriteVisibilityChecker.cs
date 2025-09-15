using UnityEngine;

public class SpriteVisibilityChecker : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool autoCheck = true;
    public bool showDebugInfo = true;

    private Camera gameCamera;
    private GameManager gameManager;

    private void Start()
    {
        gameCamera = Camera.main;
        if (gameCamera == null)
        {
            gameCamera = FindObjectOfType<Camera>();
        }

        gameManager = FindObjectOfType<GameManager>();

        if (autoCheck)
        {
            Invoke("CheckSpriteVisibility", 1f); // Check after game setup
        }
    }

    [ContextMenu("Check Sprite Visibility")]
    public void CheckSpriteVisibility()
    {
        Debug.Log("=== SPRITE VISIBILITY CHECK ===");

        // Check camera setup
        CheckCameraSetup();

        // Check card sprites
        CheckCardSprites();

        // Check if sprites are in camera view
        CheckSpritesInCameraView();

        Debug.Log("==============================");
    }

    private void CheckCameraSetup()
    {
        if (gameCamera == null)
        {
            Debug.LogError("❌ No camera found!");
            return;
        }

        Debug.Log($"📹 Camera: {gameCamera.name}");
        Debug.Log($"  Position: {gameCamera.transform.position}");
        Debug.Log($"  Orthographic: {gameCamera.orthographic}");
        Debug.Log($"  Orthographic Size: {gameCamera.orthographicSize}");
        Debug.Log($"  Culling Mask: {LayerMaskToString(gameCamera.cullingMask)}");

        if (!gameCamera.orthographic)
        {
            Debug.LogWarning("⚠️ Camera is not orthographic! For 2D sprites, set camera to orthographic.");
        }

        if (gameCamera.transform.position.z >= 0)
        {
            Debug.LogWarning("⚠️ Camera Z position should be negative (e.g., -10) to see sprites at Z=0");
        }
    }

    private void CheckCardSprites()
    {
        if (gameManager?.cardParent == null)
        {
            Debug.LogError("❌ No card parent found!");
            return;
        }

        Debug.Log($"🃏 Cards found: {gameManager.cardParent.childCount}");

        int visibleCards = 0;
        int cardsWithSprites = 0;

        foreach (Transform cardTransform in gameManager.cardParent)
        {
            SpriteRenderer sr = cardTransform.GetComponent<SpriteRenderer>();

            if (sr == null)
            {
                Debug.LogError($"❌ Card {cardTransform.name} has no SpriteRenderer!");
                continue;
            }

            cardsWithSprites++;

            if (sr.sprite == null)
            {
                Debug.LogError($"❌ Card {cardTransform.name} has no sprite assigned!");
                continue;
            }

            if (sr.color.a <= 0)
            {
                Debug.LogWarning($"⚠️ Card {cardTransform.name} has transparent color: {sr.color}");
                continue;
            }

            if (sr.enabled)
            {
                visibleCards++;

                if (showDebugInfo)
                {
                    Debug.Log($"  ✅ {cardTransform.name}: Sprite={sr.sprite.name}, Color={sr.color}, Layer={sr.sortingLayerName}, Order={sr}");
                }
            }
        }

        Debug.Log($"📊 Sprite Summary: {visibleCards}/{cardsWithSprites} cards have visible sprites");
    }

    private void CheckSpritesInCameraView()
    {
        if (gameCamera == null || gameManager?.cardParent == null) return;

        // Calculate camera bounds
        float camHeight = gameCamera.orthographic ? gameCamera.orthographicSize * 2 : 10;
        float camWidth = camHeight * gameCamera.aspect;

        Vector3 camPos = gameCamera.transform.position;
        Bounds cameraBounds = new Bounds(camPos, new Vector3(camWidth, camHeight, 20));

        Debug.Log($"📐 Camera Bounds: Center={cameraBounds.center}, Size={cameraBounds.size}");

        int cardsInView = 0;

        foreach (Transform cardTransform in gameManager.cardParent)
        {
            Vector3 cardPos = cardTransform.position;

            if (cameraBounds.Contains(cardPos))
            {
                cardsInView++;
                if (showDebugInfo)
                {
                    Debug.Log($"  ✅ Card at {cardPos} is in camera view");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log($"  ❌ Card at {cardPos} is OUTSIDE camera view");
                }
            }
        }

        Debug.Log($"👁️ Cards in camera view: {cardsInView}/{gameManager.cardParent.childCount}");

        if (cardsInView == 0 && gameManager.cardParent.childCount > 0)
        {
            Debug.LogError("❌ NO CARDS ARE VISIBLE! Check camera position and card positions.");
            SuggestFixes();
        }
    }

    private void SuggestFixes()
    {
        Debug.Log("🔧 SUGGESTED FIXES:");
        Debug.Log("1. Make sure camera is at position (0, 0, -10)");
        Debug.Log("2. Make sure card parent is at position (0, 0, 0)");
        Debug.Log("3. Make sure camera is orthographic");
        Debug.Log("4. Increase camera orthographic size if cards are too far");
        Debug.Log("5. Check if card sprites are assigned");
        Debug.Log("6. Make sure cards are at Z position 0");
    }

    private string LayerMaskToString(LayerMask layerMask)
    {
        if (layerMask == -1) return "Everything";

        string layers = "";
        for (int i = 0; i < 32; i++)
        {
            if ((layerMask & (1 << i)) != 0)
            {
                if (layers.Length > 0) layers += ", ";
                layers += LayerMask.LayerToName(i);
            }
        }
        return layers.Length > 0 ? layers : "Nothing";
    }

    [ContextMenu("Fix Common Issues")]
    public void FixCommonIssues()
    {
        Debug.Log("🔧 Attempting to fix common sprite visibility issues...");

        // Fix camera setup
        if (gameCamera != null)
        {
            gameCamera.orthographic = true;
            gameCamera.transform.position = new Vector3(0, 0, -10);
            gameCamera.orthographicSize = 6f;
            Debug.Log("✅ Fixed camera setup");
        }

        // Fix card positions and sprites
        if (gameManager?.cardParent != null)
        {
            gameManager.cardParent.position = Vector3.zero;

            foreach (Transform cardTransform in gameManager.cardParent)
            {
                // Fix Z position
                Vector3 pos = cardTransform.position;
                pos.z = 0f;
                cardTransform.position = pos;

                // Fix sprite renderer
                SpriteRenderer sr = cardTransform.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.white;
                    sr.sortingOrder = 0;
                }
            }

            Debug.Log("✅ Fixed card positions and sprite renderers");
        }

        Debug.Log("🎯 Common issues fixed! Try checking visibility again.");
    }
}