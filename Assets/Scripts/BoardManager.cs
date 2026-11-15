using UnityEngine;
using System.Collections;
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

    [Header("Board Timing")]
    [SerializeField] private float matchDelay = 0.25f;
    [SerializeField] private float fallDelay = 0.15f;

    public bool isBusy { get; private set; }

    [Header("Level")]
    [SerializeField] private LevelManager levelManager;

    [Header("Obstacles")]
    [SerializeField] private BreakableTile obstablePrefab;

    [SerializeField] private Vector2Int[] obstacleCells = {new Vector2Int(3, 3), new Vector2Int(4, 3), new Vector2Int(3, 4) };
    private BreakableTile[,] obstacles;


    public int Width => width;
    public int Height => height;

    private void Awake()
    {
        //creates board grid 
        board = new Piece[width, height];

        obstacles = new BreakableTile[width, height];
    }

    private void Start()
    {
        //creates a board on start with the given size of the board
        CreateBoard();

        CreateObstacles();

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

    private void CreateObstacles()
    {
        if(obstablePrefab == null)
        {
            return;
        }

        foreach (Vector2Int position in obstacleCells)
        {
            //skips any positions outside board 
            if (!IsInsideBoard(position))
            {
                continue;
            }

            //avoiding spawning two obstacles in the same position
            if (obstacles[position.x, position.y] != null)
            {
                continue;
            }

            Vector3 spawnPosition = new Vector3(position.x, position.y, 0f);

            BreakableTile newObstacle = Instantiate(obstablePrefab, spawnPosition, Quaternion.identity, transform);

            obstacles[position.x, position.y] = newObstacle;
        }
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

        //get pieces before swap
        Piece firstPiece = board[firstPosition.x, firstPosition.y];

        Piece secondPiece = board[secondPosition.x, secondPosition.y];

        bool firstIsColorClear = firstPiece.powerupType == PowerupType.ClearColor;

        bool secondIsColorClear = secondPiece.powerupType == PowerupType.ClearColor;

        //Come back to this // color clear + color clear combo
        if (firstIsColorClear && secondIsColorClear)
        {
            //Animate the powerups swapping
            SwapPieces(firstPosition, secondPosition);

            yield return new WaitUntil(() => !ArePiecesMoving());

            //uses 1 move
            levelManager.UseMove();

            yield return StartCoroutine(ResolveDoubleColorClearSwap());

            isBusy=false;
            yield break;
        }

        Piece colorClearPiece = null;

        if (firstIsColorClear)
        {
            colorClearPiece = firstPiece;
        }
        else if (secondIsColorClear)
        {
            colorClearPiece = secondPiece;
        }

        //store other tokens color:
        TokenType selectedType = firstIsColorClear ? secondPiece.TokenType : firstPiece.TokenType;


        SwapPieces(firstPosition, secondPosition);

        yield return new WaitUntil(() => !ArePiecesMoving());

        //color clear activates without needing a normal match
        if (colorClearPiece != null)
        {
            levelManager.UseMove();

            yield return StartCoroutine(ResolveColorClearSwap(colorClearPiece, selectedType));

            isBusy = false;
            yield break;
        }

        List <Piece> matches = matchDetector.FindMatches(board, width, height);
        
        if (matches.Count > 0)
        {
            //uses a move from remaining moves
            levelManager.UseMove();

            yield return StartCoroutine(ResolveBoard(true, firstPosition, secondPosition));
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

    private void DamageObstaclesOnMatches(List<Piece> matches)
    {
        foreach (Piece piece in matches)
        {
            if (piece == null)
            {
                continue;
            }

            Vector2Int position = piece.gridposition;

            if (!IsInsideBoard(position)) continue;

            BreakableTile obstacle = obstacles[position.x, position.y];

            if (obstacle == null) continue;

            bool destroyed = obstacle.TakeHit();

            if (destroyed)
            {
                obstacles[position.x, position.y] = null;

                levelManager.AddClearedObstacle();
            }
        }
    }

    private IEnumerator ResolveBoard(bool awardPoints, Vector2Int? firstSwap = null, Vector2Int? secondSwap = null)
    {
        isBusy = true;

        yield return new WaitUntil(() => !ArePiecesMoving());

        List<Piece> matches = matchDetector.FindMatches(board, width, height);

        bool firstMatchResolution = true;

        while(matches.Count > 0)
        {
            //give player time to see the match 
            yield return new WaitForSeconds(matchDelay);

            //remember all original matched positions
            //including token that may turn into powerup
            List<Piece> originalMatches = new List<Piece>(matches);

            if (awardPoints)
            {
                if (firstMatchResolution && firstSwap.HasValue && secondSwap.HasValue)
                {
                    //try creating bomb first
                    bool bombCreated = TryCreateBombPowerup(matches, secondSwap.Value, firstSwap.Value);

                    //if no bomb was created try creating line powerup
                    if (!bombCreated)
                    {
                        TryCreateLinePowerup(matches, firstSwap.Value, secondSwap.Value);
                    }
                }
                else if (!firstMatchResolution || !firstSwap.HasValue || !secondSwap.HasValue)
                {
                    //cascades can also create bombs
                    bool bombCreated = TryCreateBombPowerup(matches);

                    //otherwise create horizontal/vertical powerup
                    if (!bombCreated)
                    {
                        TryCreateCascadePowerUp(matches);
                    }
                }

                //activate existing powerups within the match
                ExpandPowerupClears(matches);

                //Obstacles take damage in all affected positions
                //including the surviving new powerups position
                List<Piece> obstacleHits = new List<Piece>(originalMatches);

                foreach (Piece piece in matches)
                {
                    if (piece != null && !obstacleHits.Contains(piece))
                    {
                        obstacleHits.Add(piece);
                    }
                }

                DamageObstaclesOnMatches(obstacleHits);
            }

            firstMatchResolution = false;

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

    private IEnumerator ResolveColorClearSwap(Piece colorClearPiece, TokenType selectedType)
    {

        //Starts the color clear with tokens self
        List<Piece> piecesToClear = new List<Piece> { colorClearPiece };

        //this collects every token in the selected color
        //aswell as activates all the powerups selected
        ExpandPowerupClears(piecesToClear, colorClearPiece, selectedType);

        //pause before destruction
        yield return new WaitForSeconds(matchDelay);

        //damagae obstacles affected by tokens
        DamageObstaclesOnMatches(piecesToClear);

        //award points for every cleared token
        levelManager.AddClearedTokens(piecesToClear.Count);

        //updated collection objectives
        foreach (Piece piece in piecesToClear)
        {
            if(piece != null)
            {
                levelManager.AddCollectedToken(piece.TokenType);
            }
        }

        //destroy the color clear and affacted tokens
        DestroyMatches(piecesToClear);

        yield return new WaitForSeconds(fallDelay);

        //existing method refill
        CollapseBoard();

        yield return new WaitUntil(() => !ArePiecesMoving());

        RefillBoard();

        yield return new WaitUntil(() => !ArePiecesMoving());

        yield return StartCoroutine(ResolveBoard(true));

    }
    
    private IEnumerator ResolveDoubleColorClearSwap()
    {
        List<Piece> allPieces = new List<Piece>();

        //collect every token on the board
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Piece piece = board[x, y];

                if (piece != null)
                {
                    allPieces.Add(piece);
                }
            }
        }

        //give player a second to see combo
        yield return new WaitForSeconds(matchDelay);

        //Damage obstacles
        DamageObstaclesOnMatches(allPieces);

        //awward score
        levelManager.AddClearedTokens(allPieces.Count);

        //count all destroyed tokens toward the collection objectives 
        foreach (Piece piece in allPieces)
        {
            if (piece != null)
            {
                levelManager.AddCollectedToken(piece.TokenType);
            }
        }

        //destroy every token
        DestroyMatches(allPieces);

        //show empty board
        yield return new WaitForSeconds(fallDelay);

        //refill board
        RefillBoard();

        yield return new WaitUntil(() => !ArePiecesMoving());

        //resolve any matches from refill
        //also win/lose check
        yield return StartCoroutine(ResolveBoard(true));
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

        bool firstColorClear = first.powerupType == PowerupType.ClearColor;

        bool secondColorClear = second.powerupType == PowerupType.ClearColor;

        if(firstColorClear || secondColorClear)
        {
            return true;
        }

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

    private int CountMatchingTokens(Vector2Int position, Vector2Int direction, TokenType type)
    {
        int count = 0;

        Vector2Int checkPosition = position + direction;

        while (IsInsideBoard(checkPosition))
        {
            Piece piece = board[checkPosition.x, checkPosition.y];

            if (piece == null || piece.TokenType != type) break;

            count++;

            checkPosition += direction;
        }

        return count;
    }

    private PowerupType GetFourMatchPowerup(Vector2Int position)
    {
        Piece piece = GetPiece(position);

        if (piece == null) return PowerupType.none;

        TokenType type = piece.TokenType;

        //count matched tokens horizontally
        int horizontalCount = 1;

        horizontalCount += CountMatchingTokens(position, Vector2Int.left, type);

        horizontalCount += CountMatchingTokens(position, Vector2Int.right, type);

        //count matched tokens vertically
        int verticalCount = 1;

        verticalCount += CountMatchingTokens(position, Vector2Int.up, type);

        verticalCount += CountMatchingTokens(position, Vector2Int.down, type);

        if (horizontalCount >= 4) return PowerupType.Horizontal;

        if (verticalCount >= 4) return PowerupType.Vertical;

        return PowerupType.none;
    }

    private void TryCreateLinePowerup(List<Piece> matches, Vector2Int firstPosition, Vector2Int secondPosition)
    {
        Vector2Int[] positions = {secondPosition, firstPosition};

        foreach (Vector2Int position in positions)
        {
            Piece piece = GetPiece(position);

            if (piece == null) continue;

            if (!matches.Contains(piece)) continue;

            //Dont overwrite an existing powerup
            if (piece.powerupType != PowerupType.none) continue;

            PowerupType type = GetFourMatchPowerup(position);

            if (type == PowerupType.none) continue;

            //this piece is the left over piece from a 4 match
            matches.Remove(piece);

            //turn the piece into a powerup
            piece.SetPowerupType(type);

            //Debug.Log("Created " + type + " powerup at " + position);

            return;
        }
    }

    private void TryCreateCascadePowerUp(List<Piece> matches)
    {
        Piece powerupPiece = null;
        PowerupType powerupType = PowerupType.none;

        //check all tokens involved in the cascade match
        foreach (Piece piece in matches)
        {
            if (piece == null) continue;

            //dont replace existing powerups
            if (piece.powerupType != PowerupType.none) continue;

            //check for if token is vertical or horizontal match of 4+
            PowerupType type = GetFourMatchPowerup(piece.gridposition);

            if (type == PowerupType.none) continue;

            powerupPiece = piece;
            powerupType = type;
            break;
        }

        //no match of for found
        if (powerupPiece == null) return;

        //keep one token alive to become the powerup
        matches.Remove(powerupPiece);

        //Give it the powerup and its visual prefab
        powerupPiece.SetPowerupType(powerupType);
    }

    private bool TryCreateBombPowerup(List<Piece> matches, Vector2Int? perferredPosition = null, Vector2Int? alternatePosition = null)
    {
        List<Piece> candidates = new List<Piece>();

        //for player swaps, perfer the swapped positions.
        if (perferredPosition.HasValue)
        {
            candidates.Add(GetPiece(perferredPosition.Value));
        }

        if (alternatePosition.HasValue)
        {
            candidates.Add(GetPiece(alternatePosition.Value));
        }

        //also inspect every matched piece, allows bomb to form during cascades
        candidates.AddRange(matches);

        foreach (Piece piece in matches)
        {
            if (piece == null) continue;

            if (!matches.Contains(piece)) continue;

            //dont replace existing powerup
            if (piece.powerupType != PowerupType.none) continue;

            Vector2Int position = piece.gridposition;
            TokenType type = piece.TokenType;

            //count connected tokens horizontally
            int horizontalCount = 1;

            horizontalCount += CountMatchingTokens(position, Vector2Int.left, type);

            horizontalCount += CountMatchingTokens(position, Vector2Int.right, type);

            //count connected tokens vertically
            int verticalCount = 1;

            verticalCount += CountMatchingTokens(position, Vector2Int.up, type);

            verticalCount += CountMatchingTokens(position, Vector2Int.down, type);


            //Given a 'T' or 'L' intersection has at least 3 matching pieces in both directions
            if(horizontalCount >= 3 && verticalCount >= 3)
            {
                //keep the intersection piece alive
                matches.Remove(piece);

                //convert to a bomb
                piece.SetPowerupType(PowerupType.Bomb);

                return true;
            }
        }

        return false;
    }


    private void ExpandPowerupClears(List<Piece> matches, Piece selectedColorClear = null, TokenType? selectedType = null)
    {
        //this tracks tokens that are going to be destroyed
        HashSet<Piece> AffectedPieces = new HashSet<Piece>(matches);

        Queue<Piece> powerupQueue = new Queue<Piece>(matches);

        while(powerupQueue.Count > 0) 
        {
            Piece piece = powerupQueue.Dequeue();

            if (piece == null || piece.powerupType == PowerupType.none)
            {
                continue;
            }

            Vector2Int position = piece.gridposition;

            //horizontal powerup::
            if (piece.powerupType == PowerupType.Horizontal)
            {
                for (int x = 0; x < width; x++)
                {
                    Piece target = board[x, position.y];

                    if (target != null && AffectedPieces.Add(target))
                    {
                        powerupQueue.Enqueue(target);
                    }
                }

                Debug.Log("HorizontalPowerup Activated");
            }

            //vertical powerup::
            else if (piece.powerupType == PowerupType.Vertical)
            {
                for (int y = 0; y < height; y++)
                {
                    Piece target = board[position.x, y];

                    if(target != null && AffectedPieces.Add(target))
                    {
                        powerupQueue.Enqueue(target);
                    }
                }
                Debug.Log("Vertical Powerup Activated");
            }
            else if (piece.powerupType == PowerupType.Bomb)
            {
                //Destroy every token ina 3x3 area
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        Vector2Int targetPosition = new Vector2Int(position.x + offsetX, position.y + offsetY);

                        //ignore grid pos outside board
                        if (!IsInsideBoard(targetPosition)) continue;

                        Piece target = board[targetPosition.x, targetPosition.y];

                        if (target != null && AffectedPieces.Add(target))
                        {
                            //Chain into any other powerups
                            powerupQueue.Enqueue(target);
                        }
                    }
                }
            }
            else if (piece.powerupType == PowerupType.ClearColor)
            {
                //for a direct swap use that other tokens color,
                //but for a chained activation use this tokens own color
                TokenType colorToClear = piece == selectedColorClear && selectedType.HasValue ? selectedType.Value : piece.TokenType;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Piece target = board[x, y];

                        if (target == null) continue;

                        if (target.TokenType != colorToClear) continue;

                        if (AffectedPieces.Add(target))
                        {
                            //other powerups caught in the clear also activate as part of the chain
                            powerupQueue.Enqueue(target);
                        }
                    }
                }
            }
        }

        //replace the match list with all the tokens affected by powerup
        matches.Clear();
        matches.AddRange(AffectedPieces);
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

    [ContextMenu("Test Hit Obstacle")]
    private void TestHitObstacle()
    {
        if (!Application.isPlaying || isBusy) return;

        foreach (Vector2Int position in obstacleCells)
        {
            if (!IsInsideBoard(position)) continue;

            BreakableTile obstacle = obstacles[position.x, position.y];

            if(obstacle == null) continue;

            bool destroyed = obstacle.TakeHit();

            if (destroyed)
            {
                obstacles[position.x, position.y] = null;
            }

            return;
        }
    }



}


