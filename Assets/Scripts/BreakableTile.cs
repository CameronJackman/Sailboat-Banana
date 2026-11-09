
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BreakableTile : MonoBehaviour
{
    [Header("Durability")]
    [SerializeField, Min(1)] private int startingHealth = 2;

    [Header("Temporary Visuals")]
    [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.65f, 1f, 0.55f);

    [SerializeField] private Color damagedColor = new Color(0.8f, 0.9f, 1f, 0.25f);

    public int remainingHits { get; private set; }

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        remainingHits = Mathf.Max(1, startingHealth);

        UpdateAppearance();
    }
        

    //bool that returns true when tile is destroyed
    public bool TakeHit()
    {
        if (remainingHits <= 0) return true;

        remainingHits--;

        if (remainingHits <= 0)
        {
            Destroy(gameObject);
            return true;
        }

        UpdateAppearance();
        return false;
    }

    private void UpdateAppearance()
    {
        if (remainingHits >= startingHealth)
        {
            spriteRenderer.color = fullHealthColor;
        }
        else
        {
            spriteRenderer.color = damagedColor;
        }
    }
}
