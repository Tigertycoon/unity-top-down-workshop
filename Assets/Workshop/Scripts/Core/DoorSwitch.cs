using UnityEngine;
using System.Collections.Generic;

namespace Workshop
{
    /// <summary>
    /// Einfaches Schalter/Tür-System für die 90- und 120-Minuten-Variante.
    /// Ein Schalter kann eine oder mehrere Türen öffnen.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DoorSwitch : MonoBehaviour
    {
        [Header("Typ")]
        public SwitchType switchType = SwitchType.Switch;

        [Header("Verbundene Türen (nur für Schalter)")]
        [Tooltip("Türen die dieser Schalter öffnet")]
        public List<DoorSwitch> connectedDoors = new List<DoorSwitch>();

        [Header("Tür-Einstellungen")]
        [Tooltip("Benötigt einen Schlüssel zum Öffnen")]
        public bool requiresKey = false;

        [Header("Visuals")]
        [Tooltip("Sprite wenn aktiviert/geöffnet")]
        public Sprite activatedSprite;
        [Tooltip("Sprite wenn deaktiviert/geschlossen")]
        public Sprite deactivatedSprite;

        [Header("Animation")]
        public float openSpeed = 3f;
        public Vector3 openOffset = Vector3.up * 1f; // Tür gleitet nach oben

        // Status
        private bool isActivated = false;
        private SpriteRenderer spriteRenderer;
        private Collider2D blockingCollider;
        private Vector3 closedPosition;
        private Vector3 openPosition;

        public bool IsActivated => isActivated;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            blockingCollider = GetComponent<Collider2D>();

            closedPosition = transform.position;
            openPosition = closedPosition + openOffset;

            if (switchType == SwitchType.Switch)
            {
                blockingCollider.isTrigger = true;
            }

            UpdateVisuals();
        }

        private void Update()
        {
            // Türen animieren
            if (switchType == SwitchType.Door)
            {
                Vector3 targetPos = isActivated ? openPosition : closedPosition;
                transform.position = Vector3.MoveTowards(
                    transform.position, targetPos, openSpeed * Time.deltaTime);

                // Collider deaktivieren wenn offen
                if (blockingCollider != null && !blockingCollider.isTrigger)
                    blockingCollider.enabled = !isActivated;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (switchType != SwitchType.Switch) return;
            if (isActivated) return;

            var player = other.GetComponent<PlayerController>();
            if (player == null) return;

            // Schlüssel prüfen
            if (requiresKey)
            {
                var mission = MissionManager.Instance;
                if (mission == null) return;

                bool hasKey = mission.HasCollectedType(CollectibleType.Key);

                if (!hasKey)
                {
                    WorkshopUI.Instance?.ShowHint("Du brauchst einen Schlüssel!");
                    AudioManager.Instance?.PlaySFX("Locked");
                    return;
                }
            }

            Activate();
        }

        public void Activate()
        {
            isActivated = true;
            UpdateVisuals();
            AudioManager.Instance?.PlaySFX("Switch");

            // Verbundene Türen öffnen
            foreach (var door in connectedDoors)
            {
                if (door != null)
                    door.Open();
            }
        }

        public void Open()
        {
            isActivated = true;
            UpdateVisuals();
            AudioManager.Instance?.PlaySFX("DoorOpen");
        }

        public void Close()
        {
            isActivated = false;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer == null) return;

            if (isActivated && activatedSprite != null)
                spriteRenderer.sprite = activatedSprite;
            else if (!isActivated && deactivatedSprite != null)
                spriteRenderer.sprite = deactivatedSprite;
        }

        /// <summary>Reset für Level-Neustart</summary>
        public void ResetDoorSwitch()
        {
            isActivated = false;
            transform.position = closedPosition;
            UpdateVisuals();

            if (switchType == SwitchType.Door && blockingCollider != null)
                blockingCollider.enabled = true;
        }

        private void OnDrawGizmos()
        {
            // Verbindungen zwischen Schalter und Türen zeichnen
            if (switchType == SwitchType.Switch)
            {
                Gizmos.color = Color.cyan;
                foreach (var door in connectedDoors)
                {
                    if (door != null)
                        Gizmos.DrawLine(transform.position, door.transform.position);
                }
            }

            // Schalter-Symbol
            Gizmos.color = switchType == SwitchType.Switch ?
                new Color(1f, 0.8f, 0f, 0.6f) :
                new Color(0.6f, 0.3f, 0f, 0.6f);

            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);
        }
    }

    public enum SwitchType
    {
        Switch,  // Der Spieler betritt den Trigger = Schalter wird aktiviert
        Door     // Wird von einem Schalter gesteuert, blockiert den Weg
    }
}
