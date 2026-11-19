using UnityEngine;

public class PowerupVisualAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform animatedRoot;
    [SerializeField] private Transform iconTransform;

    [Header("Idle Animation")]
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseAmount = 0.08f;

    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.05f;

    [SerializeField] private float iconRotateSpeed = 12f;

    [Header("Spawn Pop")]
    [SerializeField] private float spawnPopDuration = 0.18f;
    [SerializeField] private float spawnPopScaleMultiplier = 1.25f;

    private Vector3 rootBaseScale;
    private Vector3 rootBasePosition;
    private Vector3 iconBaseScale;

    private float spawnTimer;
    private bool playSpawnPop;

    private void Awake()
    {
        if (animatedRoot == null)
            animatedRoot = transform;

        rootBaseScale = animatedRoot.localScale;
        rootBasePosition = animatedRoot.localPosition;

        if (iconTransform != null)
        {
            iconBaseScale = iconTransform.localScale;
        }
    }

    private void Update()
    {
        float time = Time.time;

        //idle pulse
        float pulse = 1f + Mathf.Sin(time * pulseSpeed) * pulseAmount;
        animatedRoot.localScale = rootBaseScale * pulse;

        //idle bob
        float bob = Mathf.Sin(time * bobSpeed) * bobAmount;
        animatedRoot.localPosition = rootBasePosition + new Vector3(0f, bob, 0f);

        //slight icon rotation
        if (iconTransform != null)
        {
            iconTransform.localRotation =
                Quaternion.Euler(0f, 0f, Mathf.Sin(time * iconRotateSpeed * 0.1f) * 6f);

            iconTransform.localScale = iconBaseScale;
        }

        //spawn pop
        if (playSpawnPop)
        {
            spawnTimer += Time.deltaTime;
            float t = spawnTimer / spawnPopDuration;

            if (t >= 1f)
            {
                playSpawnPop = false;
                spawnTimer = 0f;
            }
            else
            {
                float pop = Mathf.Lerp(
                    spawnPopScaleMultiplier,
                    1f,
                    t
                );

                animatedRoot.localScale = rootBaseScale * pop;
            }
        }
    }

    public void PlaySpawnPop()
    {
        playSpawnPop = true;
        spawnTimer = 0f;
    }
}