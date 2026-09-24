using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CloudHop.Editor
{
    public static class CharacterArtInstaller
    {
        private const string Root = "Assets/_Project/";
        [MenuItem("Cloud Hop/Apply Uploaded Adventurer")]
        public static void Install()
        {
            string[] names = { "Idle", "Charge", "Jump", "Fall" };
            var sprites = new Sprite[4];
            for (int i = 0; i < names.Length; i++)
            {
                string path = Root + "Art/Characters/CloudAdventurer/" + names[i] + ".png";
                // Inspect alpha solely to align each pose's lowest visible pixel with the feet.
                var texture = new Texture2D(2, 2);
                texture.LoadImage(File.ReadAllBytes(path));
                Color32[] pixels = texture.GetPixels32();
                int bottom = texture.height;
                for (int y = 0; y < texture.height; y++)
                    for (int x = 0; x < texture.width; x++)
                        if (pixels[y * texture.width + x].a > 32) bottom = Mathf.Min(bottom, y);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 1100;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.63f, (float)bottom / texture.height);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                Object.DestroyImmediate(texture);
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            string dataPath = Root + "ScriptableObjects/CloudAdventurer.asset";
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>(dataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<CharacterData>();
                AssetDatabase.CreateAsset(data, dataPath);
            }
            data.displayName = "Cloud Adventurer";
            data.sprite = sprites[0];
            data.chargeSprite = sprites[1];
            data.jumpSprite = sprites[2];
            data.fallSprite = sprites[3];
            data.tint = Color.white;
            data.overrideVisualTransform = true;
            data.visualScale = Vector3.one;
            data.visualOffset = new Vector3(0, -0.45f, 0);
            EditorUtility.SetDirty(data);
            string prefabPath = Root + "Prefabs/Characters/Player.prefab";
            var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            SetSkin(prefab.GetComponentInChildren<CharacterVisual>(true), data);
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            foreach (string path in Directory.GetFiles(Root + "Scenes", "*.unity"))
            {
                var scene = EditorSceneManager.OpenScene(path);
                foreach (var go in scene.GetRootGameObjects())
                    foreach (var visual in go.GetComponentsInChildren<CharacterVisual>(true))
                        SetSkin(visual, data);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }

        private static void SetSkin(CharacterVisual visual, CharacterData data)
        {
            var serialized = new SerializedObject(visual);
            serialized.FindProperty("character").objectReferenceValue = data;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            visual.Apply(data);
            EditorUtility.SetDirty(visual);
        }
    }
}