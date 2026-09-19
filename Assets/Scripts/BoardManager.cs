using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    //board size
    [Header("Board Size")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;

    //piece prefabs
    [Header("Piece")]
    [SerializeField] private GameObject piecePrefab;

    private Piece[,] board;

    //match referense
    [Header("Match Detection")]
    [SerializeField] private MatchDetector matchDetector;

    private void Awake()
    {
        //creates board grid 
        board = new Piece[width, height];
    }

    private void Start()
    {
        //creates a board on start with the given size of the board
        CreateBoard();
    }

    private void CreateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreatePiece(x, y);
            }
        }

        TestForMatches();
    }

    //creates a piece for every grid point 
    private void CreatePiece(int x, int y)
    {
        TokenType type = GetRandomToken();

        Vector3 worldPos = new Vector3(x, y, 0);

        GameObject pieceObj = Instantiate(
            piecePrefab,
            worldPos,
            Quaternion.identity
        );

        Piece piece = pieceObj.GetComponent<Piece>();

        piece.SetTokenType(type);
        piece.gridposition = new Vector2Int(x, y);

        board[x, y] = piece;

    }

    //gets a random token type when creating a piece
    private TokenType GetRandomToken()
    {
        int randomIndex = Random.Range(0, 6);

        return (TokenType)randomIndex;
    }


    //the check for any matches 
    private void TestForMatches()
    {
        List<Piece> matches =
            matchDetector.FindMatches(board, width, height);

        Debug.Log("Matches found: " + matches.Count);

        foreach (Piece piece in matches)
        {
            Debug.Log( "Match at: " + piece.gridposition + " - " + piece.TokenType);

        }
    }

    public Piece GetPiece(Vector2Int position)
    {
        if (!IsInsideBoard(position))
            return null;

        return board[position.x, position.y];
    }

    private bool IsInsideBoard(Vector2Int position)
    {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    }


    public void TrySwap(Vector2Int firstPosition, Vector2Int secondPosition)
    {
        if(!IsInsideBoard(firstPosition) || !IsInsideBoard(secondPosition))
        {
            return;
        }

        Piece firstPiece = board[firstPosition.x, firstPosition.y];

        Piece secondPiece = board[secondPosition.x, secondPosition.y];

        if (firstPiece == null || secondPiece == null)
            return;

        SwapPieces(firstPosition, secondPosition);
    }


    private void SwapPieces(Vector2Int firstPosition, Vector2Int secondPosition)
    {
        Piece firstPiece = board[firstPosition.x, firstPosition.y];

        Piece secondPiece = board[secondPosition.x, secondPosition.y];

        board[firstPosition.x, firstPosition.y] = secondPiece;

        board[secondPosition.x, secondPosition.y] = firstPiece;

        firstPiece.gridposition = secondPosition;
        secondPiece.gridposition = firstPosition;

        Vector3 firstWorldPosition = new Vector3(firstPosition.x, firstPosition.y, 0);

        Vector3 secondWorldPosition = new Vector3(secondPosition.x, secondPosition.y, 0);

        firstPiece.transform.position = secondWorldPosition;
        secondPiece.transform.position = firstWorldPosition;
    }








}


