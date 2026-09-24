using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CloudHop.Editor
{
    public static class PlatformArtInstaller
    {
        private const string Root = "Assets/_Project/";
        private static readonly Sprite[] Sprites = new Sprite[8];
        private static readonly float[] Widths = new float[8];
        public static void Install()
        {
            // Top surface (not the decorative leaves) defines the contact line.
            float[] surfaceFromTop = { .40f, .41f, .40f, .49f, .33f, .43f, .48f, .40f };
            for (int i = 0; i < 8; i++)
            {
                string path = Root + "Art/Platforms/Platform" + (i + 1) + ".png";
                var texture = new Texture2D(2,2);
                texture.LoadImage(File.ReadAllBytes(path));
                int min = texture.width, max = 0;
                var pixels = texture.GetPixels32();
                for (int y = 0; y < texture.height; y++)
                    for (int x = 0; x < texture.width; x++)
                        if (pixels[y * texture.width + x].a > 32)
                        { min = Mathf.Min(min,x); max = Mathf.Max(max,x); }
                Widths[i] = (max-min+1)/100f;
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2((min+max)*0.5f/texture.width, 1-surfaceFromTop[i]);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
                Object.DestroyImmediate(texture);
                Sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            string prefabPath = Root+"Prefabs/Platforms/Platform.prefab";
            var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            Apply(prefab.GetComponent<Platform>(),0);
            PrefabUtility.SaveAsPrefabAsset(prefab,prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            foreach (var path in Directory.GetFiles(Root+"Scenes","*.unity"))
            {
                var scene = EditorSceneManager.OpenScene(path);
                int courseIndex = 0;
                foreach(var root in scene.GetRootGameObjects())
                    foreach(var course in root.GetComponentsInChildren<StageCourse>(true))
                    {
                        for(int n=0;n<course.Platforms.Length;n++)
                        {
                            var platform=course.Platforms[n];
                            float width=platform.GetComponent<BoxCollider2D>().size.x * platform.transform.lossyScale.x;
                            int index = courseIndex == 0 ? (width >= 3.5f ? 3 : n%3)
                                : courseIndex == 1 ? (n%3 == 0 ? 7 : n%2 == 0 ? 4 : 1)
                                : (n%4 == 0 ? 7 : n%2 == 0 ? 5 : 6);
                            Apply(platform,index);
                        }
                        courseIndex++;
                    }
                foreach(var root in scene.GetRootGameObjects())
                    foreach(var platform in root.GetComponentsInChildren<Platform>(true))
                        if(platform.GetComponentInParent<StageCourse>(true) == null) Apply(platform,0);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }

        private static void Apply(Platform platform,int index)
        {
            var root=platform.gameObject;
            var child=root.transform.Find("PlatformArt");
            if(child == null)
            {
                child=new GameObject("PlatformArt").transform;
                child.SetParent(root.transform,false);
            }
            var renderer=child.GetComponent<SpriteRenderer>();
            if(renderer==null)renderer=child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite=Sprites[index];
            renderer.sharedMaterial=root.GetComponent<SpriteRenderer>().sharedMaterial;
            var art=root.GetComponent<PlatformArtwork>();
            if(art==null)art=root.AddComponent<PlatformArtwork>();
            art.Configure(renderer,Widths[index]);
                        EditorUtility.SetDirty(art);
            EditorUtility.SetDirty(renderer);
            if (PrefabUtility.IsPartOfPrefabInstance(root))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                PrefabUtility.RecordPrefabInstancePropertyModifications(art);
                PrefabUtility.RecordPrefabInstancePropertyModifications(child);
            }
        }
    }
}
