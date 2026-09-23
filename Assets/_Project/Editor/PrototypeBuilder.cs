using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CloudHop.Editor
{
    public static class PrototypeBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/CloudHopPrototype.unity";
        [MenuItem("Cloud Hop/Create Prototype (only if missing)")]
        public static void Build()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Cloud Hop prototype already exists; no assets overwritten."); return; }
            string[] folders = { "Art", "Audio", "Prefabs/Characters", "Prefabs/Platforms", "Prefabs/UI", "Scenes", "ScriptableObjects", "Tests/PlayMode" };
            foreach (string folder in folders) Directory.CreateDirectory("Assets/_Project/" + folder);
            AssetDatabase.Refresh();
            if (Application.isBatchMode)
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            Scene previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var texture = new Texture2D(32, 32);
                var pixels = new Color[32 * 32];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                texture.SetPixels(pixels);
                texture.Apply();
                const string imagePath = "Assets/_Project/Art/PrototypeSquare.png";
                File.WriteAllBytes(imagePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(imagePath);
                var importer = (TextureImporter)AssetImporter.GetAtPath(imagePath);
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
                var material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
                AssetDatabase.CreateAsset(material, "Assets/_Project/Art/PrototypeUnlit.mat");
                var skin = ScriptableObject.CreateInstance<CharacterData>();
                skin.sprite = sprite;
                skin.tint = new Color(1f, 0.75f, 0.25f);
                AssetDatabase.CreateAsset(skin, "Assets/_Project/ScriptableObjects/PrototypeCharacter.asset");

                var platformObject = new GameObject("Platform", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Platform));
                platformObject.GetComponent<SpriteRenderer>().sprite = sprite;
                platformObject.GetComponent<BoxCollider2D>().size = Vector2.one;
                platformObject.GetComponent<BoxCollider2D>().size = Vector2.one;
                platformObject.GetComponent<SpriteRenderer>().sharedMaterial = material;
                platformObject.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.94f, 1f);
                var platformPrefab = PrefabUtility.SaveAsPrefabAsset(platformObject, "Assets/_Project/Prefabs/Platforms/Platform.prefab");
                Object.DestroyImmediate(platformObject);
                Vector3[] positions = {
                    new Vector3(0, -0.3f), new Vector3(3.6f, 0.15f), new Vector3(7.5f, -0.25f),
                    new Vector3(11.5f, 0.55f), new Vector3(15.5f, 0), new Vector3(19.5f, 0.6f),
                    new Vector3(23.4f, -0.3f), new Vector3(27.4f, 0.3f), new Vector3(31.4f, 0.8f),
                    new Vector3(35.4f, 0f), new Vector3(39.4f, 0.4f), new Vector3(43.4f, -0.2f)
                };
                var platforms = new GameObject("Platforms");
                for (int i = 0; i < positions.Length; i++)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(platformPrefab, scene);
                    instance.name = i == 0 ? "Start Platform" : "Platform " + i;
                    instance.transform.SetParent(platforms.transform);
                    instance.transform.position = positions[i];
                    instance.transform.localScale = new Vector3(i == 0 ? 3.2f : (i % 3 == 0 ? 1.8f : 2.4f), 0.6f, 1f);
                    if (i == 0) Set(instance.GetComponent<Platform>(), "awardsScore", false);
                }
                var playerObject = new GameObject("Player", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(ChargeInput), typeof(PlayerController));
                var body = playerObject.GetComponent<Rigidbody2D>();
                body.gravityScale = 2f;
                body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
                playerObject.GetComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.9f);
                var visual = new GameObject("Visual", typeof(SpriteRenderer), typeof(Animator), typeof(CharacterVisual));
                visual.transform.SetParent(playerObject.transform, false);
                visual.transform.localScale = new Vector3(0.6f, 0.9f, 1);
                var renderer = visual.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = skin.tint;
                renderer.sharedMaterial = material;
                renderer.sortingOrder = 1;
                Set(visual.GetComponent<CharacterVisual>(), "character", skin);
                Set(visual.GetComponent<CharacterVisual>(), "spriteRenderer", renderer);
                Set(visual.GetComponent<CharacterVisual>(), "animator", visual.GetComponent<Animator>());
                var playerPrefab = PrefabUtility.SaveAsPrefabAsset(playerObject, "Assets/_Project/Prefabs/Characters/Player.prefab");
                Object.DestroyImmediate(playerObject);
                playerObject = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
                playerObject.transform.position = new Vector3(0, 0.48f, 0);
                var player = playerObject.GetComponent<PlayerController>();
                var spawn = new GameObject("Spawn Point").transform;
                spawn.position = playerObject.transform.position;

                var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(FollowCamera));
                cameraObject.tag = "MainCamera";
                var camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.13f, 0.23f, 0.36f);
                cameraObject.transform.position = new Vector3(3, 1, -10);
                var follow = cameraObject.GetComponent<FollowCamera>();
                Set(follow, "target", player.transform);
                var systems = new GameObject("Game Systems", typeof(ScoreManager), typeof(GameManager));
                var scores = systems.GetComponent<ScoreManager>();
                var game = systems.GetComponent<GameManager>();
                Set(game, "player", player); Set(game, "scores", scores);
                Set(game, "spawnPoint", spawn); Set(game, "followCamera", follow);

                var canvasObject = new GameObject("Prototype UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(PrototypeHUD));
                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(960, 600);
                scaler.matchWidthOrHeight = 0.5f;
                var title = Label("Title", canvasObject.transform, "CLOUD HOP", new Vector2(0, 255), new Vector2(900, 50), 32);
                var score = Label("Score", canvasObject.transform, "SCORE : 0", new Vector2(0, 205), new Vector2(900, 40), 24);
                var charge = Label("Charge Hint", canvasObject.transform, "HOLD SPACE / LEFT CLICK  -  RELEASE TO JUMP", new Vector2(0, -245), new Vector2(920, 45), 21);
                Label("Course Hint", canvasObject.transform, "12 PLATFORM TEST COURSE", new Vector2(0, -280), new Vector2(900, 25), 14);
                var panel = new GameObject("Game Over Panel", typeof(RectTransform), typeof(Image));
                panel.transform.SetParent(canvasObject.transform, false);
                panel.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 340);
                panel.GetComponent<Image>().color = new Color(0.05f, 0.1f, 0.18f, 0.98f);
                var result = Label("Result", panel.transform, "GAME OVER", new Vector2(0, 45), new Vector2(400, 200), 30);
                var buttonObject = new GameObject("Retry", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(panel.transform, false);
                var rect = buttonObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(220, 58); rect.anchoredPosition = new Vector2(0, -105);
                buttonObject.GetComponent<Image>().color = new Color(0.14f, 0.48f, 0.65f);
                var button = buttonObject.GetComponent<Button>();
                button.targetGraphic = buttonObject.GetComponent<Image>();
                // Space is exclusively gameplay input; avoid a selected Retry consuming it.
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                Label("Label", buttonObject.transform, "RETRY", Vector2.zero, new Vector2(220, 58), 24);
                var hud = canvasObject.GetComponent<PrototypeHUD>();
                Set(hud, "game", game); Set(hud, "scores", scores); Set(hud, "player", player);
                Set(hud, "scoreText", score); Set(hud, "chargeText", charge); Set(hud, "resultText", result);
                Set(hud, "gameOverPanel", panel); Set(hud, "retryButton", button);
                panel.SetActive(false);
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                Debug.Log("CLOUD_HOP_BUILD_OK: " + ScenePath);
            }
            finally
            {
                if (previous.IsValid()) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size, int fontSize)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = size; rect.anchoredPosition = position;
            var text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize; text.text = value; text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white; text.raycastTarget = false;
            return text;
        }
        private static void Set(Object target, string name, Object value)
        {
            var data = new SerializedObject(target);
            data.FindProperty(name).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Set(Object target, string name, bool value)
        {
            var data = new SerializedObject(target);
            data.FindProperty(name).boolValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
