using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;

    private Piece selectedPiece;

    private Vector2 touchStartPosition;
    private bool isTouching;

    private void Update()
    {
        HandleMouseInput();
        HandleTouchInput();
    }


    //vvvvvvvvvvvvvv mouse input vvvvvvvvvvvvvvvvvv

    private void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x),
                Mathf.RoundToInt(worldPos.y));

            HandlePieceSelection(gridPos);
        
    }



    //vvvvvvvvvvvvvvvv touch/mobile input vvvvvvvvvvvvvvvv

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
            return;

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPosition = touch.position;
                isTouching = true;
            }
            if (touch.phase == TouchPhase.Ended && isTouching)
            {
                Vector2 swipe = touch.position - touchStartPosition;

                isTouching = false;

                if (swipe.magnitude < 50f)
                    return;

                Vector2 direction = swipe.normalized;

                Vector2Int gridDirection;

                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    gridDirection = direction.x > 0
                        ? Vector2Int.right : Vector2Int.left;
                }
                else
                {
                    gridDirection = direction.y > 0
                        ? Vector2Int.up : Vector2Int.down;
                }

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchStartPosition);

                Vector2Int startPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));

                boardManager.TrySwap(startPos, startPos + gridDirection);
        }
    }



    // vvvvvvvvvvvv Selection vvvvvvvvvvvv

    private void HandlePieceSelection(Vector2Int position)
    {
        if(selectedPiece == null){

            selectedPiece = boardManager.GetPiece(position);

            if (selectedPiece != null)
            {
                Debug.Log("Selected: " + selectedPiece.TokenType);
            }
            return;
        }

        Vector2Int selectedPos = selectedPiece.gridposition;

        Vector2Int difference = position - selectedPos;

        if(Mathf.Abs(difference.x) + Mathf.Abs(difference.y) == 1)
        {
            boardManager.TrySwap(
                selectedPos, position
                );
        }

        selectedPiece = null;
    }
}
