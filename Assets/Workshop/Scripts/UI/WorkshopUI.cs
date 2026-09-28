using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Workshop
{
    /// <summary>
    /// Haupt-UI für das Workshop-Spiel.
    /// Verwaltet HUD, Win-Screen, Intro und Hinweise.
    ///
    /// AUFBAU: Erstellt die UI-Elemente per Code, damit Kinder keine
    /// Canvas-Hierarchie verstehen müssen. Einfach dieses Script
    /// auf ein leeres GameObject legen — der Rest passiert automatisch.
    /// </summary>
    public class WorkshopUI : MonoBehaviour
    {
        public static WorkshopUI Instance { get; private set; }

        [Header("Schriftgröße")]
        [Range(16, 48)]
        public int fontSize = 28;

        [Header("Farben")]
        public Color hudColor = Color.white;
        public Color hintColor = new Color(1f, 0.9f, 0.3f);
        public Color winColor = new Color(0.2f, 1f, 0.4f);

        // --- UI-Elemente (werden per Code erzeugt) ---
        private Canvas canvas;
        private Text collectibleText;
        private Text missionNameText;
        private Text hintText;
        private GameObject winPanel;
        private Text winTitleText;
        private Text winDetailsText;
        private GameObject introPanel;
        private Text introTitleText;
        private Text introDescText;

        // Hinweis-Timer
        private Coroutine hintCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
        }

        private void BuildUI()
        {
            // Canvas erstellen
            var canvasGO = new GameObject("WorkshopCanvas");
            canvasGO.transform.SetParent(transform);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // === HUD (oben links) ===
            var hudPanel = CreatePanel(canvasGO, "HUD",
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(20, -20), new Vector2(400, 80));

            // Missionsname
            missionNameText = CreateText(hudPanel, "MissionName", "",
                fontSize - 4, hudColor, TextAnchor.UpperLeft,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Sammelobjekte-Zähler
            collectibleText = CreateText(hudPanel, "CollectibleCounter", "0 / 0",
                fontSize, hudColor, TextAnchor.LowerLeft,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // === Hinweis-Text (Mitte unten) ===
            hintText = CreateText(canvasGO, "HintText", "",
                fontSize + 4, hintColor, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f),
                Vector2.zero, new Vector2(800, 60));
            AddOutline(hintText.gameObject);
            hintText.gameObject.SetActive(false);

            // === Intro-Panel ===
            introPanel = CreateDarkPanel(canvasGO, "IntroPanel");

            introTitleText = CreateText(introPanel, "IntroTitle", "Mission",
                fontSize + 12, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f),
                Vector2.zero, new Vector2(900, 80));
            AddOutline(introTitleText.gameObject);

            introDescText = CreateText(introPanel, "IntroDesc", "Beschreibung",
                fontSize, new Color(0.9f, 0.9f, 0.9f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f),
                Vector2.zero, new Vector2(800, 100));

            introPanel.SetActive(false);

            // === Win-Screen ===
            winPanel = CreateDarkPanel(canvasGO, "WinPanel");

            winTitleText = CreateText(winPanel, "WinTitle", "MISSION GESCHAFFT!",
                fontSize + 16, winColor, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f),
                Vector2.zero, new Vector2(900, 80));
            AddOutline(winTitleText.gameObject);

            winDetailsText = CreateText(winPanel, "WinDetails", "",
                fontSize, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f),
                Vector2.zero, new Vector2(800, 100));

            var restartHint = CreateText(winPanel, "RestartHint", "Drücke [R] zum Neustart",
                fontSize - 4, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f),
                Vector2.zero, new Vector2(600, 50));

            winPanel.SetActive(false);
        }

        private void Update()
        {
            // Neustart über R-Taste
            if (winPanel.activeSelf && Input.GetKeyDown(KeyCode.R))
            {
                MissionManager.Instance?.ResetMission();
            }
        }

        // === Öffentliche Methoden ===

        public void UpdateCollectibleCounter(int collected, int total)
        {
            if (collectibleText != null)
                collectibleText.text = $"★ {collected} / {total}";
        }

        public void SetMissionName(string name)
        {
            if (missionNameText != null)
                missionNameText.text = name;
        }

        public void ShowMissionIntro(string title, string description, float duration)
        {
            if (introPanel == null) return;

            introTitleText.text = title;
            introDescText.text = description;
            introPanel.SetActive(true);

            if (missionNameText != null)
                missionNameText.text = title;

            StartCoroutine(HideAfterDelay(introPanel, duration));
        }

        public void ShowHint(string message, float duration = 2.5f)
        {
            if (hintText == null) return;

            if (hintCoroutine != null)
                StopCoroutine(hintCoroutine);

            hintText.text = message;
            hintText.gameObject.SetActive(true);
            hintCoroutine = StartCoroutine(HideAfterDelay(hintText.gameObject, duration));
        }

        public void ShowWinScreen(string missionName, int collected, int total)
        {
            if (winPanel == null) return;

            winTitleText.text = $"{missionName}\nGESCHAFFT!";
            winDetailsText.text = $"Du hast {collected} von {total} Objekten gesammelt!";
            winPanel.SetActive(true);
        }

        public void HideWinScreen()
        {
            if (winPanel != null)
                winPanel.SetActive(false);
        }

        // === Hilfsmethoden für UI-Erstellung ===

        private IEnumerator HideAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null) obj.SetActive(false);
        }

        private GameObject CreatePanel(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            rt.pivot = anchorMin;
            return go;
        }

        private GameObject CreateDarkPanel(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.75f);
            return go;
        }

        private Text CreateText(GameObject parent, string name, string text,
            int size, Color color, TextAnchor anchor,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;

            return t;
        }

        private void AddOutline(GameObject textGO)
        {
            var outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(2, -2);
        }
    }
}
