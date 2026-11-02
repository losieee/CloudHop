using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CloudHop.Editor
{
    public static class FinishFlagInstaller
    {
        public const string SpritePath = "Assets/_Project/Art/Finish/FinishFlag.png";
        public const string PrefabPath = "Assets/_Project/Prefabs/Platforms/FinishFlag.prefab";

        // Batch preview of the actual end platforms without the clear-screen overlay.
        public static void CapturePreviews()
        {
            EditorSceneManager.OpenScene(PlatformBuilds.SharedScene);
            var courses = Object.FindObjectsByType<StageCourse>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(c => c.Definition.name).ToArray();
            var camera = Camera.main;
            System.IO.Directory.CreateDirectory("Logs/finish-flag-preview");
            for (int i = 0; i < courses.Length; i++)
            {
                foreach (var course in courses) course.gameObject.SetActive(course == courses[i]);
                var finish = courses[i].Platforms.Last();
                var flags = courses[i].GetComponentsInChildren<SpriteRenderer>(true).Where(r => r.name == "Finish Flag").ToArray();
                if (flags.Length != 1 || flags[0].transform.parent != finish.transform || flags[0].GetComponent<Collider2D>() != null)
                    throw new System.InvalidOperationException("Each course must have one non-colliding flag on its final platform.");
                var position = finish.transform.position;
                camera.transform.position = new Vector3(position.x - 2, position.y + 2, -10);
                camera.orthographicSize = 4.5f;
                var target = new RenderTexture(1280, 800, 24);
                target.Create();
                var previous = RenderTexture.active;
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                var image = new Texture2D(1280, 800, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0);image.Apply();
                System.IO.File.WriteAllBytes("Logs/finish-flag-preview/stage-" + (i + 1) + ".png", image.EncodeToPNG());
                camera.targetTexture = null;RenderTexture.active = previous;
                Object.DestroyImmediate(image);target.Release();Object.DestroyImmediate(target);
            }
            Debug.Log("FINISH_FLAG_PREVIEWS_VERIFIED: " + courses.Length);
        }

        [MenuItem("Cloud Hop/맵/종점 깃발 적용")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            // Opaque flag is 1406 px high. Anchor the pole's foot, not transparent image margins.
            importer.spritePixelsPerUnit = 1406f / 2.5f;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(350f / 1024f, 63f / 1536f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (sprite == null) throw new System.InvalidOperationException("Finish flag sprite could not be imported.");
            var template = new GameObject("Finish Flag");
            var renderer = template.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 3;
            var prefab = PrefabUtility.SaveAsPrefabAsset(template, PrefabPath);
            Object.DestroyImmediate(template);

            var scene = EditorSceneManager.OpenScene(PlatformBuilds.SharedScene);
            var courses = Object.FindObjectsByType<StageCourse>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var course in courses)
            {
                var finish = course.Platforms.Last();
                var existing = finish.transform.Find("Finish Flag");
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
                var flag = (GameObject)PrefabUtility.InstantiatePrefab(prefab, finish.transform);
                flag.name = "Finish Flag";
                var surface = finish.GetComponent<BoxCollider2D>();
                flag.transform.localPosition = new Vector3(surface.offset.x + surface.size.x * .12f,
                    surface.offset.y + surface.size.y * .5f, 0);
                var scale = finish.transform.lossyScale;
                flag.transform.localScale = new Vector3(1f / Mathf.Abs(scale.x), 1f / Mathf.Abs(scale.y), 1);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("FINISH_FLAGS_INSTALLED: " + courses.Length);
        }
    }
}
