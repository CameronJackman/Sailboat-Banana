using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Mathematics;

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
        int randomIndex = UnityEngine.Random.Range(0, 6);

        return (TokenType)randomIndex;
    }


    //the check for any matches 
    private void TestForMatches()
    {
        List<Piece> matches =
            matchDetector.FindMatches(board, width, height);

        //Debug.Log("Matches found: " + matches.Count);

        foreach (Piece piece in matches)
        {
            //Debug.Log( "Match at: " + piece.gridposition + " - " + piece.TokenType);

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
        //checks if the piece swapped is inside the game grid
        if(!IsInsideBoard(firstPosition) || !IsInsideBoard(secondPosition))
        {
            return;
        }


        //making sure the two positions are next to each other 
        int distnace = Mathf.Abs(firstPosition.x - secondPosition.x) + Mathf.Abs(firstPosition.y - secondPosition.y);

        //if piece isnt 1 piece away the program will not swap
        if (distnace != 1)
        {
            return;
        }


        //after pass checks put the swaping pieces into proper variables
        Piece firstPiece = board[firstPosition.x, firstPosition.y];
        Piece secondPiece = board[secondPosition.x, secondPosition.y];

        //check if its null so no null selection is passed breaking the game
        if (firstPiece == null || secondPiece == null)
        {
            return;
        }
           
        //performs the swaps
        SwapPieces(firstPosition, secondPosition);

        //Check if swapping created a line up of 3 or more or a match
        List<Piece> matches = matchDetector.FindMatches(board, width, height);

        if (matches.Count > 0)
        {
            Debug.Log("Valid swap");

            DestroyMatches(matches);
        }
        else
        {
            Debug.Log("Invalid Swap, (swap pieces back)");

            //no match therefore return the swap
            SwapPieces(firstPosition, secondPosition);
        }
    }

    //destroy any current matches
    private void DestroyMatches(List<Piece> matches)
    {
        foreach (Piece piece in matches)
        {
            Vector2Int position = piece.gridposition;

            //remove piece from array
            board[position.x, position.y] = null;

            //Destroy gameobj
            Destroy(piece.gameObject);
        }

        CollapseBoard();
        RefillBoard();

        CheckForAfterMatches();
    }

    //Check for matches that are made after a match that was made 
    private void CheckForAfterMatches()
    {
        List<Piece> matches = matchDetector.FindMatches(board, width, height);

        if (matches.Count > 0)
        {
            DestroyMatches(matches );
        }
    }


    //this adds more candy to baord after any matches
    private void CollapseBoard()
    {
        for (int x = 0; x <width; x ++)
        {
            int emptySpaces = 0;

            for (int y = 0; y <height; y++)
            {
                if (board[x, y] == null)
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    //move piece down
                    Piece piece = board[x, y];

                    int newY = y- emptySpaces;

                    board[x, newY] = piece;
                    board[x, y] = null;

                    piece.gridposition = new Vector2Int(x, newY);

                    piece.transform.position = new Vector3(x, newY, 0);
                }
            }
        }
    }

    //refill the board once matches are made and pieces are removed
    private void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y <height; y++)
            {
                if(board[x, y] == null)
                {
                    CreatePiece(x, y);
                }
            }
        }
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


