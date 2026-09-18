using UnityEngine;



public class Piece : MonoBehaviour
{
    [Header("Token Type")]
    public TokenType TokenType;

    [Header("Powerup")]
    public PowerupType powerupType;

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
    Orange
}


