using UnityEngine;
using System.Collections;
using System.Reflection.Metadata.Ecma335;
using JetBrains.Annotations;


public class Piece : MonoBehaviour
{
    [Header("Token Type")]
    public TokenType TokenType;

    [Header("Powerup")]
    public PowerupType powerupType;

    [Header("Powerup Visual Prefabs")]
    [SerializeField] private GameObject horizontalPowerupVisualPrefab;
    [SerializeField] private GameObject verticalPowerupVisualPrefab;
    private GameObject currentPowerupVisual;

    [Header("Grid Position")]
    public Vector2Int gridposition;

    [Header("Token Sprites")]
    [SerializeField] private Sprite redSprite;
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite greenSprite;
    [SerializeField] private Sprite yellowSprite;
    [SerializeField] private Sprite purpleSprite;
    [SerializeField] private Sprite orangeSprite;

    private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.2f;

    public bool IsMoving { get; private set; }

    private Coroutine moveCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //method for setting the color of the token
    public void SetTokenType(TokenType type)
    {
        TokenType = type;

        spriteRenderer.sprite = GetSpriteForToken(type);
    }

    public void SetPowerupType(PowerupType type)
    {
        powerupType = type;

        if (currentPowerupVisual != null)
        {
            Destroy(currentPowerupVisual);
            currentPowerupVisual = null;
        }

        GameObject prefabToSpawn = null;

        if (type == PowerupType.Horizontal)
        {
            prefabToSpawn = horizontalPowerupVisualPrefab;
        }
        else if (type == PowerupType.Vertical)
        {
            prefabToSpawn = verticalPowerupVisualPrefab;
        }

        if (prefabToSpawn == null) return;

        currentPowerupVisual = Instantiate(prefabToSpawn, transform);

        currentPowerupVisual.transform.localPosition = Vector3.zero;

        //currentPowerupVisual.transform.localRotation = Quaternion.identity;
    }

    //this is what actually sets each type
    private Sprite GetSpriteForToken(TokenType type)
    {
        switch (type) 
        {
            case TokenType.Red:
                return redSprite;

            case TokenType.Blue:
                return blueSprite;

            case TokenType.Green:
                return greenSprite;

            case TokenType.Yellow:
                return yellowSprite;

            case TokenType.Purple:
                return purpleSprite;

            case TokenType.Orange:
                return orangeSprite;

            default:
                return null;
        }
    }


    public void MoveTo(Vector3 targetPosition)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (moveDuration <= 0f || Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.position = targetPosition; 
            IsMoving = false;
            return;
        }

        moveCoroutine = StartCoroutine(MoveRoutine(targetPosition));
    }

    private IEnumerator MoveRoutine(Vector3 targetPosition)
    {
        IsMoving = true;

        Vector3 startPosition = transform.position;
        float elapsed = 0f; 

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / moveDuration);

            //eases the movement in and out
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);

            yield return null;
        }

        transform.position = targetPosition;
        IsMoving= false;
        moveCoroutine = null;
    }
}

//enums to describe what each tokens identitiy 
public enum TokenType
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange
}

public enum PowerupType
{
    none,
    Horizontal,
    Vertical,
    Bomb,
    ClearColor
}


