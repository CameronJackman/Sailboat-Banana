using UnityEngine;

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

public class Piece : MonoBehaviour
{
    public TokenType TokenType;
    public PowerupType powerupType;

    public Vector2Int gridposition; 
}


