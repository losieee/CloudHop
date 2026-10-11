using UnityEditor;
using UnityEngine;
namespace CloudHop.Editor
{
    public static class LobbyLogoInstaller
    {
        [InitializeOnLoadMethod]
        private static void InstallWhenMissing()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                var assets = AssetDatabase.LoadAssetAtPath<UIAssets>("Assets/_Project/ScriptableObjects/GameUIAssets.asset");
                if (assets != null && assets.lobbyLogo == null) Install();
            };
        }
        [MenuItem("Cloud Hop/Apply English Lobby Logo")]
        public static void Install()
        {
            const string path = "Assets/_Project/Art/UI/CloudHopLogoEnglish.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var assets = AssetDatabase.LoadAssetAtPath<UIAssets>("Assets/_Project/ScriptableObjects/GameUIAssets.asset");
            assets.lobbyLogo = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (assets.lobbyLogo == null) throw new System.InvalidOperationException("Logo sprite failed to import");
            EditorUtility.SetDirty(assets);
            AssetDatabase.SaveAssets();
            Debug.Log("English lobby logo installed.");
        }
    }
}
