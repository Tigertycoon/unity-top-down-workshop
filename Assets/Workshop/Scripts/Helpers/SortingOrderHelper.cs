using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Setzt den Sorting Order automatisch basierend auf der Y-Position.
    /// Das sorgt dafür, dass Objekte weiter unten VOR Objekten weiter oben
    /// gezeichnet werden — wichtig für den Top-Down-Look.
    ///
    /// Einfach auf jedes Sprite legen, das korrekt sortiert werden soll.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SortingOrderHelper : MonoBehaviour
    {
        [Tooltip("Basis-Sortierungsebene")]
        public int baseSortingOrder = 0;

        [Tooltip("Wie stark die Y-Position die Sortierung beeinflusst")]
        public float sortingPrecision = 10f;

        [Tooltip("Nur einmal setzen (für statische Objekte) oder jeden Frame aktualisieren")]
        public bool isStatic = true;

        private SpriteRenderer sr;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            UpdateSortingOrder();
        }

        private void LateUpdate()
        {
            if (!isStatic)
                UpdateSortingOrder();
        }

        private void UpdateSortingOrder()
        {
            sr.sortingOrder = baseSortingOrder - Mathf.RoundToInt(transform.position.y * sortingPrecision);
        }
    }
}
