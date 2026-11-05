using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

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

    [Header("Board Timing")]
    [SerializeField] private float matchDelay = 0.25f;
    [SerializeField] private float fallDelay = 0.15f;

    public bool isBusy { get; private set; }

    [Header("Level")]
    [SerializeField] private LevelManager levelManager;

    public int Width => width;
    public int Height => height;

    private void Awake()
    {
        //creates board grid 
        board = new Piece[width, height];
    }

    private void Start()
    {
        //creates a board on start with the given size of the board
        CreateBoard();
        //clears starting matches without awarding points
        StartCoroutine(ResolveBoard(false));
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
    private void CreatePiece(int x, int y, int spawnOffset = 0)
    {
        TokenType type = GetRandomToken();

        Vector3 worldPos = new Vector3(x, y + spawnOffset, 0);

        GameObject pieceObj = Instantiate(
            piecePrefab,
            worldPos,
            Quaternion.identity
        );

        Piece piece = pieceObj.GetComponent<Piece>();

        piece.SetTokenType(type);
        piece.gridposition = new Vector2Int(x, y);

        board[x, y] = piece;

        piece.MoveTo(new Vector3(x, y, 0));

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
        if(isBusy || !levelManager.CanPlay) return;


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
        StartCoroutine(SwapAndResolve(firstPosition, secondPosition));
    }

    private IEnumerator SwapAndResolve(Vector2Int firstPosition, Vector2Int secondPosition)
    {
        isBusy = true;

        SwapPieces(firstPosition, secondPosition);

        yield return new WaitUntil(() => !ArePiecesMoving());

        List <Piece> matches = matchDetector.FindMatches(board, width, height);
        
        if (matches.Count > 0)
        {
            //uses a move from remaining moves
            levelManager.UseMove();

            yield return StartCoroutine(ResolveBoard(true));
        }
        else
        {
            yield return new WaitForSeconds(0.1f);

            SwapPieces(firstPosition, secondPosition);

            yield return new WaitUntil(() => !ArePiecesMoving());
        }

        isBusy = false;
    }

    //destroy any current matches
    private void DestroyMatches(List<Piece> matches)
    {
        foreach (Piece piece in matches)
        {

            if (piece == null)
                continue;

            Vector2Int position = piece.gridposition;

            //remove piece from array
            board[position.x, position.y] = null;

            //Destroy gameobj
            Destroy(piece.gameObject);
        }
    }

    private IEnumerator ResolveBoard(bool awardPoints)
    {
        isBusy = true;

        yield return new WaitUntil(() => !ArePiecesMoving());

        List<Piece> matches = matchDetector.FindMatches(board, width, height);

        while(matches.Count > 0)
        {
            //give player time to see the match 
            yield return new WaitForSeconds(matchDelay);

            //player triggered matches earn points
            if (awardPoints)
            {
                //award score for all matched 
                levelManager.AddClearedTokens(matches.Count);

                //trach each collected type
                foreach (Piece piece in matches)
                {
                    if (piece != null)
                    {
                        levelManager.AddCollectedToken(piece.TokenType);
                    }
                }
            }

            DestroyMatches(matches);

            //show gaps before moving remaining pieces
            yield return new WaitForSeconds(fallDelay);

            CollapseBoard();

            yield return new WaitUntil(() => !ArePiecesMoving());

            RefillBoard();

            //let board refill before checking again
            yield return new WaitUntil(() => !ArePiecesMoving());

            matches = matchDetector.FindMatches(board, width, height);
        }

        //evaluate final result
        if (awardPoints)
        {
            levelManager.CheckLevelResult();
        }
        //reshuffle if the player can play still
        if (levelManager.CanPlay && !HasPossibleMoves())
        {
            Debug.Log("No Possible moves... Reshuffling...");

            yield return StartCoroutine(ReshuffleBoard());
        }


        isBusy = false;
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

                    piece.MoveTo(new Vector3(x, newY, 0));
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
                    CreatePiece(x, y, height);
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

        firstPiece.MoveTo(secondWorldPosition);
        secondPiece.MoveTo(firstWorldPosition);
    }

    private bool ArePiecesMoving()
    {
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y< height; y++)
            {
                Piece piece = board[x, y];

                if(piece != null && piece.IsMoving)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool HasPossibleMoves()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y =0; y < height; y++)
            {

                //this checks swaping with the token to the right
                if (x < width - 1)
                {
                    if (WouldSwapCreateMatch(x, y, x + 1, y))
                    {
                        return true;
                    }
                }

                //Checks swaping with token above.
                if (y < height - 1)
                {
                    if (WouldSwapCreateMatch(x, y, x, y+1))
                    {
                        return true;
                    }
                }

            }
        }

        return false;
    }


    private bool WouldSwapCreateMatch(int x1, int y1, int x2, int y2)
    {
        Piece first = board[x1, y1];
        Piece second = board[x2, y2];
            
        //swap identical colors changes nothing
        if (first == null || second == null) { return false; }

        //temp swap in array
        board[x1, y1] = second;
        board[x2, y2] = first;

        //Check if a match would exist
        bool createsMatch = matchDetector.FindMatches(board, width, height).Count > 0;

        //restore to the original board
        board[x1, y1] = first;
        board[x2, y2] = second;

        return createsMatch;
    }

    private IEnumerator ReshuffleBoard()
    {
        //save the original token placement
        List<Piece> originalPieces = new List<Piece>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                originalPieces.Add(board[x, y]);
            }
        }

        List<Piece> shuffledPieces = new List<Piece>(originalPieces);

        bool validArrangement = false;

        //get system to try different arrangements untill we find one
        //with no exsisteing matches but having at least one move

        for(int attempt = 0; attempt < 300; attempt++)
        {
            ShufflePieceList(shuffledPieces);

            ApplyPieceArrangement(shuffledPieces);

            bool hasMatches = matchDetector.FindMatches(board, width, height).Count > 0;

            if (!hasMatches && HasPossibleMoves())
            {
                validArrangement = true;
                break;
            }
        }

        if (!validArrangement)
        {
            //restore to original
            ApplyPieceArrangement(originalPieces);

            Debug.LogWarning("Couldn't reshuffle / Error finding suitable board");

            yield break;
        }

        Debug.Log("board reshuffled successfully");

        //animate tokens to the new positions
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y <height; y++)
            {
                Piece piece = board[x, y];

                piece.MoveTo(new Vector3(x, y, 0));
            }
        }

        //wait until animation is done
        yield return new WaitUntil(() => !ArePiecesMoving());
    }

    private void ShufflePieceList(List<Piece> pieces)
    {
        //Shuffle
        for (int i = pieces.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            Piece temp = pieces[i];

            pieces[i] = pieces[randomIndex];

            pieces[randomIndex] = temp;
        }
    }

    private void ApplyPieceArrangement(List<Piece> pieces)
    {
        int index = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Piece piece = pieces[index];

                board[x, y] = piece;

                piece.gridposition = new Vector2Int(x, y);

                index++;
            }
        }
    }



    //Reshufle Test ??????
    [ContextMenu("Test Reshuffle")]
    private void TestReshuffle()
    {
        //Debug.Log("TEST RESHUFFLE CALLED");

        //Debug.Log("Is Busy: " + isBusy);
        //Debug.Log("Can Play: " + levelManager.CanPlay);

        if (!Application.isPlaying || isBusy || !levelManager.CanPlay)
        {
            Debug.LogWarning("Reshuffle was blocked!");
            return;
        }

        //Debug.Log("Starting reshuffle...");
        StartCoroutine(TestReshuffleRoutine());
    }

    private IEnumerator TestReshuffleRoutine()
    {
        isBusy = true;

        yield return StartCoroutine(ReshuffleBoard());

        isBusy = false;
    }
}


