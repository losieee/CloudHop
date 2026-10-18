using UnityEditor;
namespace CloudHop.Editor
{
    public sealed class HudArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/_Project/Resources/HudArt/") || !assetPath.EndsWith(".png")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
        }
    }
}
