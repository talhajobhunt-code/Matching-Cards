using System.Collections;
using UnityEngine;
using System;

public class Card : MonoBehaviour
{
    [Header("Card Settings")]
    public float flipDuration = 0.3f;
    public AnimationCurve flipCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Components
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D cardCollider;
    private AudioManager audioManager;

    // Card Data
    public int cardId { get; private set; }
    public Sprite frontSprite { get; private set; }
    public Sprite backSprite { get; private set; }

    // Card State
    private CardState currentState = CardState.FaceDown;
    private bool isFlipping = false;
    private Coroutine flipCoroutine;

    // Events
    public event Action<Card> OnCardClicked;
    public event Action<Card> OnFlipComplete;

    public CardState CurrentState => currentState;
    public bool IsFlipping => isFlipping;
    public bool IsMatched => currentState == CardState.Matched;
    public bool IsFaceUp => currentState == CardState.FaceUp || currentState == CardState.Matched;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cardCollider = GetComponent<BoxCollider2D>();

        if (cardCollider == null)
        {
            cardCollider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void Initialize(int id, Sprite front, Sprite back, AudioManager audio)
    {
        cardId = id;
        frontSprite = front;
        backSprite = back;
        audioManager = audio;

        spriteRenderer.sprite = backSprite;
        currentState = CardState.FaceDown;
        isFlipping = false;

        EnableInteraction(true);
    }

    private void OnMouseDown()
    {
        if (CanFlip())
        {
            OnCardClicked?.Invoke(this);
        }
    }

    public bool CanFlip()
    {
        return !isFlipping &&
               currentState != CardState.Matched &&
               currentState != CardState.FaceUp &&
               cardCollider.enabled;
    }

    public void Flip(bool showFront, bool playSound = true)
    {
        if (isFlipping) return;

        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }

        flipCoroutine = StartCoroutine(FlipAnimation(showFront, playSound));
    }

    private IEnumerator FlipAnimation(bool showFront, bool playSound)
    {
        isFlipping = true;
        currentState = CardState.Flipping;

        if (playSound)
        {
            audioManager?.PlayFlipSound();
        }

        Vector3 startScale = transform.localScale;
        Vector3 midScale = new Vector3(0f, startScale.y, startScale.z);
        Vector3 endScale = startScale;

        float halfDuration = flipDuration * 0.5f;

        // First half - scale down to 0 on X axis
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            t = flipCurve.Evaluate(t);

            transform.localScale = Vector3.Lerp(startScale, midScale, t);
            yield return null;
        }

        // Switch sprite at the middle of animation
        spriteRenderer.sprite = showFront ? frontSprite : backSprite;

        // Second half - scale back up
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            t = flipCurve.Evaluate(t);

            transform.localScale = Vector3.Lerp(midScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;

        // Update state
        currentState = showFront ? CardState.FaceUp : CardState.FaceDown;
        isFlipping = false;

        OnFlipComplete?.Invoke(this);
    }

    public void SetMatched()
    {
        if (currentState == CardState.Matched) return;

        currentState = CardState.Matched;
        StartCoroutine(MatchedAnimation());
    }

    private IEnumerator MatchedAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 bigScale = originalScale * 1.1f;

        float duration = 0.2f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, bigScale, t);
            yield return null;
        }

        // Scale back down
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(bigScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;

        // Fade out slightly to show it's matched
        Color color = spriteRenderer.color;
        color.a = 0.7f;
        spriteRenderer.color = color;

        EnableInteraction(false);
    }

    public void EnableInteraction(bool enable)
    {
        cardCollider.enabled = enable;
    }

    public void ResetCard()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }

        currentState = CardState.FaceDown;
        isFlipping = false;
        spriteRenderer.sprite = backSprite;
        transform.localScale = Vector3.one;

        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;

        EnableInteraction(true);
    }

    private void OnDestroy()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }
    }
}

public enum CardState
{
    FaceDown,
    FaceUp,
    Flipping,
    Matched
}