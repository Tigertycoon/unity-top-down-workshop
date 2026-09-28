using System.IO;
using UnityEditor;
using UnityEngine;

namespace Workshop.EditorTools
{
    /// <summary>Reusable neutral sprite for missions that do not supply an art pack.</summary>
    public static class WorkshopShapes
    {
        private const string Path = "Assets/Workshop/Art/WorkshopSquare.png";

        public static SpriteRenderer AddRenderer(GameObject target)
        {
            var renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSquare();
            return renderer;
        }

        public static Sprite GetSquare()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Path);
            if (sprite != null) return sprite;
            Directory.CreateDirectory("Assets/Workshop/Art");
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var pixels = new Color32[256];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(Path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(Path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(Path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(Path);
        }
    }
}
