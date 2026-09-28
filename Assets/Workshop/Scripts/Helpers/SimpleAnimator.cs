using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Einfacher Code-basierter Animator für Objekte, die keine
    /// vollständige Animation brauchen. Nützlich für Deko-Objekte.
    ///
    /// Kann: Schweben, Rotieren, Pulsieren, Wackeln
    /// Alles ohne Animator-Controller — einfach drauflegen und einstellen.
    /// </summary>
    public class SimpleAnimator : MonoBehaviour
    {
        [Header("Schwebeanimation")]
        public bool bob = false;
        [Range(0f, 1f)]
        public float bobHeight = 0.2f;
        [Range(0.5f, 5f)]
        public float bobSpeed = 2f;

        [Header("Rotation")]
        public bool rotate = false;
        [Range(-180f, 180f)]
        public float rotateSpeed = 45f;

        [Header("Pulsieren (Größe)")]
        public bool pulse = false;
        [Range(0f, 0.5f)]
        public float pulseAmount = 0.1f;
        [Range(0.5f, 5f)]
        public float pulseSpeed = 2f;

        [Header("Wackeln")]
        public bool wobble = false;
        [Range(0f, 30f)]
        public float wobbleAngle = 10f;
        [Range(0.5f, 5f)]
        public float wobbleSpeed = 3f;

        private Vector3 startPosition;
        private Vector3 startScale;
        private float timeOffset;

        private void Start()
        {
            startPosition = transform.localPosition;
            startScale = transform.localScale;
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float t = Time.time + timeOffset;

            // Schweben
            if (bob)
            {
                float y = startPosition.y + Mathf.Sin(t * bobSpeed) * bobHeight;
                transform.localPosition = new Vector3(startPosition.x, y, startPosition.z);
            }

            // Rotation
            if (rotate)
            {
                transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
            }

            // Pulsieren
            if (pulse)
            {
                float scale = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
                transform.localScale = startScale * scale;
            }

            // Wackeln
            if (wobble)
            {
                float angle = Mathf.Sin(t * wobbleSpeed) * wobbleAngle;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
}
