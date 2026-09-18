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

    private void TestForMatches()
    {
        List<Piece> matches =
            matchDetector.FindMatches(board, width, height);

        Debug.Log("Matches found: " + matches.Count);
    }
    

}

//10
