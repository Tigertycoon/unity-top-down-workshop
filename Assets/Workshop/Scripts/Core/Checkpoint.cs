using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Optionaler Checkpoint. Wenn der Spieler durchläuft,
    /// wird die Rücksetzposition aktualisiert.
    /// Nützlich für größere Level in der 120-Minuten-Variante.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Visuelles Feedback")]
        public Sprite inactiveSprite;
        public Sprite activeSprite;
        public Color activeColor = new Color(0.3f, 1f, 0.5f);

        private bool activated = false;
        private Color initialColor;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) initialColor = spriteRenderer.color;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated) return;

            var player = other.GetComponent<PlayerController>();
            if (player == null) return;

            activated = true;
            player.SetCheckpoint(transform.position);

            if (spriteRenderer != null)
            {
                if (activeSprite != null) spriteRenderer.sprite = activeSprite;
                spriteRenderer.color = activeColor;
            }

            AudioManager.Instance?.PlaySFX("Checkpoint");
        }

        public void ResetCheckpoint()
        {
            activated = false;
            if (spriteRenderer != null)
            {
                if (inactiveSprite != null) spriteRenderer.sprite = inactiveSprite;
                spriteRenderer.color = initialColor;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = activated ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawIcon(transform.position, "d_Favorite Icon", true);
        }
    }
}
