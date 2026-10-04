using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace CloudHop.Editor
{
    public static class TypographyInstaller
    {
        private static void OnImported(string packageName)
        {
            AssetDatabase.importPackageCompleted -= OnImported;
            EditorApplication.delayCall += Install;
        }
        [MenuItem("CLOUD HOP/Install UI Typography")]
        public static void Install()
        {
            if (!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
            {
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Text).Assembly);
                AssetDatabase.importPackageCompleted += OnImported;
                AssetDatabase.importPackageFailed += (name, error) => { Debug.LogError(error); if(Application.isBatchMode)EditorApplication.Exit(1); };
                AssetDatabase.ImportPackage(package.resolvedPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
                return;
            }
            const string root = "Assets/_Project/Art/Fonts/";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(root + "CloudHop UI SDF.asset");
            if (font == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<Font>(root + "LilitaOne-Regular.ttf");
                font = TMP_FontAsset.CreateFontAsset(source, 90, 12, GlyphRenderMode.SDFAA, 1024, 1024);
                font.name = "CloudHop UI SDF";
                // Bake the entire current English UI alphabet; no runtime atlas growth needed.
                string characters = new string(Enumerable.Range(32, 95).Select(c => (char)c).ToArray());
                if (!font.TryAddCharacters(characters)) throw new System.Exception("UI font is missing required characters.");
                font.atlasPopulationMode = AtlasPopulationMode.Static;
                AssetDatabase.CreateAsset(font, root + "CloudHop UI SDF.asset");
                foreach (var texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture, font);
                AssetDatabase.AddObjectToAsset(font.material, font);
                EditorUtility.SetDirty(font);
            }
            var korean = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(root + "CloudHop Korean SDF.asset");
            if (korean == null)
            {
                korean = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(root + "Jua-Regular.ttf"), 90, 12, GlyphRenderMode.SDFAA, 512, 512);
                korean.name = "CloudHop Korean SDF";
                if (!korean.TryAddCharacters("점프 : 스페이스바")) throw new System.Exception("Missing Korean hint glyphs.");
                korean.atlasPopulationMode = AtlasPopulationMode.Static;
                AssetDatabase.CreateAsset(korean, root + "CloudHop Korean SDF.asset");
                foreach (var texture in korean.atlasTextures) AssetDatabase.AddObjectToAsset(texture, korean);
                AssetDatabase.AddObjectToAsset(korean.material, korean);
                EditorUtility.SetDirty(korean);
            }
            // Nicknames may contain Hangul not known when the game is built.
            korean.atlasPopulationMode=AtlasPopulationMode.Dynamic;
            korean.isMultiAtlasTexturesEnabled=true;
            EditorUtility.SetDirty(korean);
            korean.material.EnableKeyword("OUTLINE_ON");
            korean.material.EnableKeyword("UNDERLAY_ON");
            EditorUtility.SetDirty(korean.material);
            if (font.fallbackFontAssetTable == null) font.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
            if (!font.fallbackFontAssetTable.Contains(korean)) font.fallbackFontAssetTable.Add(korean);
            EditorUtility.SetDirty(font);
            // Keep these shader variants referenced in Windows/Web builds.
            font.material.EnableKeyword("OUTLINE_ON");
            font.material.EnableKeyword("UNDERLAY_ON");
            EditorUtility.SetDirty(font.material);
            var assets = AssetDatabase.LoadAssetAtPath<UIAssets>("Assets/_Project/ScriptableObjects/GameUIAssets.asset");
            assets.uiFont = font;
            EditorUtility.SetDirty(assets);
            AssetDatabase.SaveAssets();
            Debug.Log("CLOUD HOP typography installed.");
            if(Application.isBatchMode)EditorApplication.Exit(0);
        }
    }
}
