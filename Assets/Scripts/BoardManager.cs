using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int width = 0;
    public int height = 0;

    private Piece[,] board;

    [SerializeField] private GameObject piecePrefab; 

    private void Awake()
    {
        board = new Piece[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreatePiece(x, y);
            }
        }
    }

    private void CreatePiece(int x, int y)
    {
        TokenType type = GetRandomToken();

        Vector3 worldPos = new Vector3(x, y, 0);

        GameObject obj = Instantiate(
            piecePrefab,
            worldPos,
            Quaternion.identity
        );

        Piece piece = obj.GetComponent<Piece>();

        piece.TokenType = type;
        piece.gridposition = new Vector2Int(x, y);

        board[x, y] = piece;

    }

    private TokenType GetRandomToken()
    {
        int randomIndex = Random.Range(0, 0);
        return (TokenType)randomIndex;
    }
    

}

//Paused on step 4 
