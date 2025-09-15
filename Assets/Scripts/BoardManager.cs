using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public Vector2 targetArea = new Vector2(10f, 8f);
    public float minCardSize = 0.5f;
    public float maxCardSize = 2f;

    private GameManager gameManager;

    public void Initialize(GameManager gm)
    {
        gameManager = gm;
    }

    public List<Card> CreateBoard(int width, int height, Sprite[] cardImages, Sprite backSprite,
                                GameObject cardPrefab, Transform parent, float baseSpacing)
    {
        int totalCards = width * height;

        // Ensure even number of cards for pairs
        if (totalCards % 2 != 0)
        {
            Debug.LogError("Board must have even number of cards for matching pairs!");
            totalCards--;
            if (width > height)
                width--;
            else
                height--;
        }

        List<Card> cards = new List<Card>();
        List<int> cardIds = GenerateCardIds(totalCards, cardImages.Length);

        // Calculate card size and spacing based on target area
        Vector2 cardScale = CalculateCardScale(width, height, baseSpacing);
        Vector2 boardSize = CalculateBoardSize(width, height, cardScale, baseSpacing);
        Vector2 startPosition = CalculateStartPosition(boardSize);

        // Create cards
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index >= totalCards) break;

                Card card = CreateCard(cardPrefab, parent, cardIds[index], cardImages, backSprite);
                PositionCard(card, x, y, startPosition, cardScale, baseSpacing);
                cards.Add(card);
            }
        }

        return cards;
    }

    private List<int> GenerateCardIds(int totalCards, int availableImages)
    {
        int pairsNeeded = totalCards / 2;
        List<int> cardIds = new List<int>();

        // Create pairs
        for (int i = 0; i < pairsNeeded; i++)
        {
            int imageIndex = i % availableImages;
            cardIds.Add(imageIndex);
            cardIds.Add(imageIndex);
        }

        // Shuffle the cards
        ShuffleList(cardIds);

        return cardIds;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private Vector2 CalculateCardScale(int width, int height, float spacing)
    {
        // Calculate available space for cards
        float totalSpacingWidth = (width - 1) * spacing;
        float totalSpacingHeight = (height - 1) * spacing;

        float availableWidth = targetArea.x - totalSpacingWidth;
        float availableHeight = targetArea.y - totalSpacingHeight;

        // Calculate card size to fit in available space
        float cardWidth = availableWidth / width;
        float cardHeight = availableHeight / height;

        // Use the smaller dimension to maintain aspect ratio
        float cardSize = Mathf.Min(cardWidth, cardHeight);

        // Clamp to min/max sizes
        cardSize = Mathf.Clamp(cardSize, minCardSize, maxCardSize);

        Debug.Log($"Calculated card size: {cardSize} (Available: {availableWidth}x{availableHeight}, Spacing: {spacing})");

        return new Vector2(cardSize, cardSize);
    }

    private Vector2 CalculateBoardSize(int width, int height, Vector2 cardScale, float spacing)
    {
        float boardWidth = (width * cardScale.x) + ((width - 1) * spacing);
        float boardHeight = (height * cardScale.y) + ((height - 1) * spacing);
        return new Vector2(boardWidth, boardHeight);
    }

    private Vector2 CalculateStartPosition(Vector2 boardSize)
    {
        return new Vector2(-boardSize.x * 0.5f, -boardSize.y * 0.5f);
    }

    private Card CreateCard(GameObject prefab, Transform parent, int cardId, Sprite[] images, Sprite backSprite)
    {
        GameObject cardObject = Instantiate(prefab, parent);
        Card card = cardObject.GetComponent<Card>();

        if (card == null)
        {
            card = cardObject.AddComponent<Card>();
        }

        // Get the sprite for this card
        Sprite frontSprite = images[cardId % images.Length];

        // Initialize the card
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        card.Initialize(cardId, frontSprite, backSprite, audioManager);

        return card;
    }

    private void PositionCard(Card card, int x, int y, Vector2 startPos, Vector2 cardScale, float spacing)
    {
        Vector3 position = new Vector3(
            startPos.x + (x * (cardScale.x + spacing)) + (cardScale.x * 0.5f),
            startPos.y + (y * (cardScale.y + spacing)) + (cardScale.y * 0.5f),
            0
        );

        card.transform.localPosition = position;
        card.transform.localScale = new Vector3(cardScale.x, cardScale.y, 1f);
    }


    public Vector2 GetBoardBounds(int width, int height, Vector2 cardScale, float spacing)
    {
        return CalculateBoardSize(width, height, cardScale, spacing);
    }

}

[System.Serializable]
public class BoardConfiguration
{
    public int width;
    public int height;
    public float aspectRatio;
    public float aspectDifference;

    public override string ToString()
    {
        return $"{width}x{height} (AR: {aspectRatio:F2})";
    }
}