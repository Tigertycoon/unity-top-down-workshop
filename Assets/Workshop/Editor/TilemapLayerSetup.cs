using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Workshop.EditorTools
{
    /// <summary>
    /// Erstellt eine einfache, kindgerechte Tilemap-Struktur.
    /// Kinder sollen nicht selbst entscheiden muessen, welche Tilemap Collider braucht.
    /// TilemapCollider2D reicht hier aus; CompositeCollider2D waere glatter,
    /// bringt fuer den Workshop aber unnoetige Zusatzkomponenten mit.
    /// </summary>
    public static class TilemapLayerSetup
    {
        private const string GridName = "Grid";

        private static readonly LayerDefinition[] DefaultLayers =
        {
            new LayerDefinition("01 Boden - begehbar", 0, CollisionMode.None),
            new LayerDefinition("02 Haeuser und grosse Objekte - STOPPT Spieler", 8, CollisionMode.Blocking),
            new LayerDefinition("03 Wasser - STOPPT Spieler", 2, CollisionMode.Blocking),
            new LayerDefinition("04 Waende Felsen Zaun - STOPPT Spieler", 7, CollisionMode.Blocking,
                "04 Waende und Felsen - STOPPT Spieler"),
            new LayerDefinition("05 Gefahren - RESET", 4, CollisionMode.HazardReset),
            new LayerDefinition("06 Deko vorne - ohne Kollision", 20, CollisionMode.None)
        };

        [MenuItem("Workshop/Tilemap-Ebenen vorbereiten", false, 15)]
        public static void PrepareDefaultLayersFromMenu()
        {
            PrepareDefaultLayers();
        }

        public static GameObject PrepareDefaultLayers(bool createUndoGroup = true)
        {
            int undoGroup = -1;
            if (createUndoGroup)
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Tilemap-Ebenen vorbereiten");
                undoGroup = Undo.GetCurrentGroup();
            }

            var grid = FindOrCreateGrid();

            foreach (var layer in DefaultLayers)
            {
                EnsureLayer(grid.transform, layer);
            }

            var firstLayer = grid.transform.Find(DefaultLayers[0].Name);
            Selection.activeGameObject = firstLayer != null ? firstLayer.gameObject : grid.gameObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            if (createUndoGroup)
                Undo.CollapseUndoOperations(undoGroup);

            Debug.Log("[Workshop] Tilemap-Ebenen vorbereitet: Boden, Haeuser/grosse Objekte, Wasser, Waende/Felsen/Zaun, Gefahren, Deko vorne.");
            return grid.gameObject;
        }

        public static bool HasPreparedBlockingLayers()
        {
            var grids = Object.FindObjectsByType<Grid>(FindObjectsSortMode.None);
            foreach (var grid in grids)
            {
                if (HasBlockingLayer(grid.transform, "02 Haeuser und grosse Objekte - STOPPT Spieler") &&
                    HasBlockingLayer(grid.transform, "03 Wasser - STOPPT Spieler") &&
                    HasBlockingLayer(grid.transform, "04 Waende Felsen Zaun - STOPPT Spieler"))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasBlockingLayer(Transform gridTransform, string layerName)
        {
            var layer = gridTransform.Find(layerName);
            if (layer == null)
                return false;

            var collider = layer.GetComponent<TilemapCollider2D>();
            return collider != null && !collider.isTrigger;
        }

        private static Grid FindOrCreateGrid()
        {
            Grid selectedGrid = null;
            var grids = Object.FindObjectsByType<Grid>(FindObjectsSortMode.None);

            foreach (var grid in grids)
            {
                if (grid.name == GridName)
                {
                    selectedGrid = grid;
                    break;
                }
            }

            if (selectedGrid == null && grids.Length > 0)
            {
                selectedGrid = grids[0];
            }

            if (selectedGrid == null)
            {
                var gridGO = new GameObject(GridName, typeof(Grid));
                Undo.RegisterCreatedObjectUndo(gridGO, "Grid erstellt");
                selectedGrid = gridGO.GetComponent<Grid>();
            }

            Undo.RecordObject(selectedGrid, "Grid konfigurieren");
            selectedGrid.cellSize = Vector3.one;
            selectedGrid.cellGap = Vector3.zero;
            selectedGrid.cellLayout = GridLayout.CellLayout.Rectangle;
            selectedGrid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
            EditorUtility.SetDirty(selectedGrid);

            return selectedGrid;
        }

        private static void EnsureLayer(Transform gridTransform, LayerDefinition layer)
        {
            var child = FindExistingLayer(gridTransform, layer);
            GameObject layerGO;

            if (child == null)
            {
                layerGO = new GameObject(layer.Name);
                Undo.RegisterCreatedObjectUndo(layerGO, layer.Name + " erstellt");
                layerGO.transform.SetParent(gridTransform, false);
            }
            else
            {
                layerGO = child.gameObject;
                if (layerGO.name != layer.Name)
                {
                    Undo.RecordObject(layerGO, layer.Name + " umbenennen");
                    layerGO.name = layer.Name;
                }
            }

            Undo.RecordObject(layerGO.transform, layer.Name + " transformieren");
            layerGO.transform.localPosition = Vector3.zero;
            layerGO.transform.localRotation = Quaternion.identity;
            layerGO.transform.localScale = Vector3.one;

            var tilemap = EnsureComponent<Tilemap>(layerGO);
            tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

            var renderer = EnsureComponent<TilemapRenderer>(layerGO);
            Undo.RecordObject(renderer, layer.Name + " Renderer konfigurieren");
            renderer.sortOrder = TilemapRenderer.SortOrder.BottomLeft;
            renderer.mode = TilemapRenderer.Mode.Chunk;
            renderer.sortingOrder = layer.SortingOrder;
            EditorUtility.SetDirty(renderer);

            switch (layer.Collision)
            {
                case CollisionMode.Blocking:
                    ConfigureBlockingLayer(layerGO);
                    break;

                case CollisionMode.HazardReset:
                    ConfigureHazardLayer(layerGO);
                    break;
            }

            EditorUtility.SetDirty(tilemap);
            EditorUtility.SetDirty(layerGO);
        }

        private static Transform FindExistingLayer(Transform gridTransform, LayerDefinition layer)
        {
            var child = gridTransform.Find(layer.Name);
            if (child != null)
                return child;

            foreach (var oldName in layer.AlternativeNames)
            {
                child = gridTransform.Find(oldName);
                if (child != null)
                    return child;
            }

            return null;
        }

        private static void ConfigureBlockingLayer(GameObject layerGO)
        {
            RemoveComponentIfPresent<CompositeCollider2D>(layerGO);
            RemoveComponentIfPresent<Rigidbody2D>(layerGO);

            var tilemapCollider = EnsureComponent<TilemapCollider2D>(layerGO);
            Undo.RecordObject(tilemapCollider, "Tilemap Collider konfigurieren");
            tilemapCollider.isTrigger = false;
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.None;

            EditorUtility.SetDirty(tilemapCollider);
        }

        private static void ConfigureHazardLayer(GameObject layerGO)
        {
            RemoveComponentIfPresent<CompositeCollider2D>(layerGO);
            RemoveComponentIfPresent<Rigidbody2D>(layerGO);

            var tilemapCollider = EnsureComponent<TilemapCollider2D>(layerGO);
            Undo.RecordObject(tilemapCollider, "Gefahren-Collider konfigurieren");
            tilemapCollider.isTrigger = true;
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.None;

            var hazard = EnsureComponent<Workshop.Hazard>(layerGO);
            Undo.RecordObject(hazard, "Gefahrenmodus konfigurieren");
            hazard.mode = Workshop.HazardMode.Reset;
            hazard.gizmoColor = new Color(1f, 0.1f, 0.05f, 0.35f);

            EditorUtility.SetDirty(tilemapCollider);
            EditorUtility.SetDirty(hazard);
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component != null)
                return component;

            return Undo.AddComponent<T>(target);
        }

        private static void RemoveComponentIfPresent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component != null)
                Undo.DestroyObjectImmediate(component);
        }

        private readonly struct LayerDefinition
        {
            public LayerDefinition(string name, int sortingOrder, CollisionMode collision, params string[] alternativeNames)
            {
                Name = name;
                SortingOrder = sortingOrder;
                Collision = collision;
                AlternativeNames = alternativeNames ?? new string[0];
            }

            public string Name { get; }
            public int SortingOrder { get; }
            public CollisionMode Collision { get; }
            public string[] AlternativeNames { get; }
        }

        private enum CollisionMode
        {
            None,
            Blocking,
            HazardReset
        }
    }
}
