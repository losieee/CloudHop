using UnityEditor;
using UnityEngine;

namespace CloudHop.Editor
{
    public static class CharacterPoseInstaller
    {
        private const string Root = "Assets/_Project/";
        private static readonly string[] Poses = { "Idle", "Charge", "Jump", "Landing", "Fall" };

        [MenuItem("Cloud Hop/Install Additional Character Poses")]
        public static void Install()
        {
            InstallSkin("Girl", new[] { .59f, .55f, .49f, .57f, .70f });
            InstallSkin("Fox", new[] { .59f, .56f, .56f, .58f, .70f });
            AssetDatabase.SaveAssets();
            Debug.Log("Installed Girl and Fox: Idle, Charge, Jump, Landing, Fall.");
        }

        private static void InstallSkin(string skin, float[] anchors)
        {
            var sprites = new Sprite[Poses.Length];
            float pixelsPerUnit = 1000;
            for (int i = 0; i < Poses.Length; i++)
            {
                string path = Root + "Art/Characters/" + skin + "/" + Poses[i] + ".png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.isReadable = true;
                importer.SaveAndReimport();
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var pixels = texture.GetPixels32();
                int minY = texture.height, maxY = 0;
                for (int y = 0; y < texture.height; y++)
                    for (int x = 0; x < texture.width; x++)
                        if (pixels[y * texture.width + x].a > 128)
                        { minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
                // Keep one scale per skin, preserving the shorter crouching silhouette.
                if (i == 0) pixelsPerUnit = (maxY - minY + 1) / 1.2f;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(anchors[i], (float)minY / texture.height);
                importer.SetTextureSettings(settings);
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.isReadable = false;
                importer.SaveAndReimport();
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>(Root + "ScriptableObjects/" + skin + "Character.asset");
            data.sprite = sprites[0];
            data.chargeSprite = sprites[1];
            data.jumpSprite = sprites[2];
            data.landingSprite = sprites[3];
            data.fallSprite = sprites[4];
            data.landingPoseDuration = .16f;
            data.overrideVisualTransform = true;
            data.visualScale = Vector3.one;
            data.visualOffset = new Vector3(0, -.45f, 0);
            EditorUtility.SetDirty(data);
        }
    }
}
