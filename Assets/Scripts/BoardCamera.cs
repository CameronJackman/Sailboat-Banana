using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BoardCamera : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;

    [Header("Space Around Board")]
    [SerializeField, Min(0f)] private float sidePadding = 0.5f;
    [SerializeField, Min(0f)] private float topPadding = 2f;
    [SerializeField, Min(0f)] private float bottomPadding = 1f;

    private Camera boardCamera;

    private void Awake()
    {
        boardCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (boardManager == null || !boardCamera.orthographic)
            return;

        float aspect = boardCamera.aspect;

        if (aspect <= 0f)
            return;

        
        float requiredWidth =
            boardManager.Width + sidePadding * 2f;

        float requiredHeight =
            boardManager.Height + topPadding + bottomPadding;

        float sizeForHeight = requiredHeight / 2f;
        float sizeForWidth = requiredWidth / (2f * aspect);

        boardCamera.orthographicSize =
            Mathf.Max(sizeForHeight, sizeForWidth);

        float centreX = (boardManager.Width - 1) / 2f;
        float centreY = (boardManager.Height - 1) / 2f;

       
        centreY += (topPadding - bottomPadding) / 2f;

        transform.position = new Vector3(
            centreX,
            centreY,
            transform.position.z
        );
    }
}