using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CardManager : MonoBehaviour
{
    [Header("Gameplay Settings")]
    public float mismatchDelay = 1.0f;
    public int maxFlippedCards = 2;

    // Dependencies
    private GameManager gameManager;
    private AudioManager audioManager;

    // Card Management
    private List<Card> allCards = new List<Card>();
    private List<Card> flippedCards = new List<Card>();
    private bool isProcessingMatch = false;

    // Events
    public event Action<Card, Card> OnCardMatched;
    public event Action<Card, Card> OnCardMismatched;
    public event Action OnAllCardsMatched;

    // Properties
    public int MatchedPairsCount { get; private set; }
    public int TotalPairs { get; private set; }
    public bool IsProcessingMatch => isProcessingMatch;

    public void Initialize(GameManager gm, AudioManager audio)
    {
        gameManager = gm;
        audioManager = audio;
    }

    public void SetCards(List<Card> cards)
    {
        // Clean up previous cards
        foreach (Card card in allCards)
        {
            if (card != null)
            {
                card.OnCardClicked -= OnCardClicked;
                card.OnFlipComplete -= OnCardFlipComplete;
            }
        }

        allCards = cards;
        TotalPairs = allCards.Count / 2;
        MatchedPairsCount = 0;
        flippedCards.Clear();
        isProcessingMatch = false;

        // Subscribe to card events
        foreach (Card card in allCards)
        {
            card.OnCardClicked += OnCardClicked;
            card.OnFlipComplete += OnCardFlipComplete;
        }
    }

    private void OnCardClicked(Card clickedCard)
    {
        if (gameManager.CurrentState != GameState.Playing) return;
        if (!clickedCard.CanFlip()) return;
        if (flippedCards.Contains(clickedCard)) return;
        if (flippedCards.Count >= maxFlippedCards) return;

        FlipCard(clickedCard);
    }

    private void FlipCard(Card card)
    {
        card.Flip(true);
        flippedCards.Add(card);
    }

    private void OnCardFlipComplete(Card card)
    {
        if (flippedCards.Count == maxFlippedCards && !isProcessingMatch)
        {
            StartCoroutine(ProcessCardMatch());
        }
    }

    private IEnumerator ProcessCardMatch()
    {
        isProcessingMatch = true;

        // Wait a brief moment to let player see the cards
        yield return new WaitForSeconds(0.3f);

        if (flippedCards.Count == maxFlippedCards)
        {
            bool isMatch = CheckForMatch();

            if (isMatch)
            {
                yield return HandleMatch();
            }
            else
            {
                yield return HandleMismatch();
            }
        }

        isProcessingMatch = false;
    }

    private bool CheckForMatch()
    {
        if (flippedCards.Count != maxFlippedCards) return false;

        // For a 2-card match system
        if (maxFlippedCards == 2)
        {
            return flippedCards[0].cardId == flippedCards[1].cardId;
        }

        // For systems with more than 2 cards, all must match
        int firstCardId = flippedCards[0].cardId;
        return flippedCards.All(card => card.cardId == firstCardId);
    }

    private IEnumerator HandleMatch()
    {
        audioManager?.PlayMatchSound();

        // Mark cards as matched
        foreach (Card card in flippedCards)
        {
            card.SetMatched();
        }

        MatchedPairsCount++;

        // Fire match event
        if (flippedCards.Count >= 2)
        {
            OnCardMatched?.Invoke(flippedCards[0], flippedCards[1]);
        }

        flippedCards.Clear();

        // Check if game is won
        if (MatchedPairsCount >= TotalPairs)
        {
            yield return new WaitForSeconds(0.5f);
            OnAllCardsMatched?.Invoke();
        }
    }

    private IEnumerator HandleMismatch()
    {
        audioManager?.PlayMismatchSound();

        // Fire mismatch event
        if (flippedCards.Count >= 2)
        {
            OnCardMismatched?.Invoke(flippedCards[0], flippedCards[1]);
        }

        // Wait before flipping cards back
        yield return new WaitForSeconds(mismatchDelay);

        // Flip cards back to face down
        foreach (Card card in flippedCards)
        {
            if (card != null && !card.IsMatched)
            {
                card.Flip(false, false); // Don't play sound when flipping back
            }
        }

        flippedCards.Clear();
    }

    public void ResetAllCards()
    {
        flippedCards.Clear();
        MatchedPairsCount = 0;
        isProcessingMatch = false;

        foreach (Card card in allCards)
        {
            if (card != null)
            {
                card.ResetCard();
            }
        }
    }

    public List<Card> GetMatchedCards()
    {
        return allCards.Where(card => card.IsMatched).ToList();
    }

    public List<Card> GetFlippedCards()
    {
        return new List<Card>(flippedCards);
    }

    public void FlipAllCardsDown()
    {
        foreach (Card card in allCards)
        {
            if (card != null && card.IsFaceUp && !card.IsMatched)
            {
                card.Flip(false, false);
            }
        }
        flippedCards.Clear();
    }

    // For save/load functionality
    public CardSaveData GetSaveData()
    {
        CardSaveData saveData = new CardSaveData();
        saveData.matchedPairsCount = MatchedPairsCount;
        saveData.cardStates = new List<CardStateData>();

        foreach (Card card in allCards)
        {
            CardStateData cardState = new CardStateData();
            cardState.cardId = card.cardId;
            cardState.isMatched = card.IsMatched;
            cardState.isFaceUp = card.IsFaceUp && !card.IsMatched;
            saveData.cardStates.Add(cardState);
        }

        return saveData;
    }

    public void LoadFromSaveData(CardSaveData saveData)
    {
        MatchedPairsCount = saveData.matchedPairsCount;

        for (int i = 0; i < allCards.Count && i < saveData.cardStates.Count; i++)
        {
            Card card = allCards[i];
            CardStateData cardState = saveData.cardStates[i];

            if (cardState.isMatched)
            {
                card.Flip(true, false);
                card.SetMatched();
            }
            else if (cardState.isFaceUp)
            {
                card.Flip(true, false);
                flippedCards.Add(card);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (Card card in allCards)
        {
            if (card != null)
            {
                card.OnCardClicked -= OnCardClicked;
                card.OnFlipComplete -= OnCardFlipComplete;
            }
        }
    }
}

[System.Serializable]
public class CardSaveData
{
    public int matchedPairsCount;
    public List<CardStateData> cardStates;
}

[System.Serializable]
public class CardStateData
{
    public int cardId;
    public bool isMatched;
    public bool isFaceUp;
}