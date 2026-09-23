using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CloudHop.Editor
{
    public static class StageBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/CloudHopStages.unity";
        private const string DataFolder = "Assets/_Project/ScriptableObjects/Stages";

        [MenuItem("Cloud Hop/Create Three Stages (only if missing)")]
        public static void Build()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Stage scene exists; keeping authored changes."); return; }
            Directory.CreateDirectory(DataFolder);
            AssetDatabase.Refresh();
            if (Application.isBatchMode) EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            Scene previous = SceneManager.GetActiveScene();
            if (!AssetDatabase.CopyAsset(PrototypeBuilder.ScenePath, ScenePath))
                throw new System.InvalidOperationException("Could not copy prototype scene.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var definitions = new[] { CreateEasy(), CreateNormal(), CreateHard() };
            bool completed = false;
            try
            {
                var rootObjects = scene.GetRootGameObjects();
                var systems = System.Array.Find(rootObjects, x => x.name == "Game Systems");
                var ui = System.Array.Find(rootObjects, x => x.name == "Prototype UI");
                var oldPlatforms = System.Array.Find(rootObjects, x => x.name == "Platforms");
                var camera = System.Array.Find(rootObjects, x => x.name == "Main Camera").GetComponent<Camera>();
                Object.DestroyImmediate(oldPlatforms);
                camera.orthographicSize = 6;
                var director = systems.AddComponent<StageDirector>();
                var game = systems.GetComponent<GameManager>();
                Set(director, "gameCamera", camera);
                Set(game, "stages", director);
                var courses = new List<StageCourse>();
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Platforms/Platform.prefab");
                for (int s = 0; s < definitions.Length; s++)
                {
                    var definition = definitions[s];
                    var root = new GameObject(definition.stageName, typeof(StageCourse));
                    var course = root.GetComponent<StageCourse>();
                    var platforms = new List<Platform>();
                    var spawn = new GameObject("Stage Spawn").transform;
                    spawn.SetParent(root.transform);
                    spawn.position = new Vector3(0, 0.48f, 0);
                    Transform sectionRoot = null;
                    string lastSection = null;
                    for (int i = 0; i < definition.platforms.Length; i++)
                    {
                        var layout = definition.platforms[i];
                        if (lastSection != layout.section)
                        {
                            sectionRoot = new GameObject(layout.section).transform;
                            sectionRoot.SetParent(root.transform);
                            lastSection = layout.section;
                        }
                        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                        instance.name = i == 0 ? "Start" : (i == definition.platforms.Length - 1 ? "FINISH" : $"Landing {i:00}");
                        instance.transform.SetParent(sectionRoot);
                        instance.transform.position = layout.position;
                        instance.transform.localScale = new Vector3(layout.width, 0.6f, 1);
                        var platform = instance.GetComponent<Platform>();
                        if (i == 0)
                        {
                            var serialized = new SerializedObject(platform);
                            serialized.FindProperty("awardsScore").boolValue = false;
                            serialized.ApplyModifiedPropertiesWithoutUndo();
                        }
                        var renderer = instance.GetComponent<SpriteRenderer>();
                        renderer.color = i == definition.platforms.Length - 1 ? new Color(1f, 0.8f, 0.2f)
                            : (i % 8 == 0 ? new Color(0.55f, 1f, 0.75f) : definition.platformColor);
                        platforms.Add(platform);
                    }
                    Set(course, "definition", definition);
                    Set(course, "spawnPoint", spawn);
                    SetArray(course, "platforms", platforms.ToArray());
                    courses.Add(course);
                    root.SetActive(s == 0);
                }
                SetArray(director, "courses", courses.ToArray());
                camera.backgroundColor = definitions[0].skyColor;
                var hud = ui.GetComponent<PrototypeHUD>();
                Set(hud, "stages", director);
                var title = ui.transform.Find("Title").GetComponent<Text>();
                title.fontSize = 27;
                title.text = definitions[0].stageName + " / EASY";
                Set(hud, "stageText", title);
                var progress = ui.transform.Find("Course Hint").GetComponent<Text>();
                progress.text = "0 / 40 - WARM UP";
                progress.fontSize = 17;
                Set(hud, "progressText", progress);
                var panel = ui.transform.Find("Game Over Panel");
                panel.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 350);
                panel.Find("Result").GetComponent<Text>().fontSize = 27;
                var retry = panel.Find("Retry").GetComponent<Button>();
                retry.GetComponent<RectTransform>().anchoredPosition = new Vector2(-115, -105);
                retry.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 58);
                retry.GetComponentInChildren<Text>().text = "RETRY STAGE";
                retry.GetComponentInChildren<Text>().fontSize = 20;
                var next = Object.Instantiate(retry.gameObject, panel).GetComponent<Button>();
                next.name = "Next Stage";
                next.GetComponent<RectTransform>().anchoredPosition = new Vector2(115, -105);
                next.GetComponentInChildren<Text>().text = "NEXT STAGE";
                Set(hud, "nextStageButton", next);
                Set(hud, "nextStageLabel", next.GetComponentInChildren<Text>());
                next.gameObject.SetActive(false);
                for (int i = 0; i < 3; i++)
                {
                    var button = Object.Instantiate(retry.gameObject, ui.transform).GetComponent<Button>();
                    button.name = "Select Stage " + (i + 1);
                    button.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 36);
                    button.GetComponent<RectTransform>().anchoredPosition = new Vector2((i - 1) * 185, -193);
                    var label = button.GetComponentInChildren<Text>();
                    label.rectTransform.sizeDelta = new Vector2(170, 36);
                    label.fontSize = 17;
                    label.text = (i + 1) + "  " + definitions[i].difficulty;
                    UnityEventTools.AddIntPersistentListener(button.onClick, game.SelectStage, i);
                }
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                completed = true;
                Debug.Log("CLOUD_HOP_STAGES_CREATED: 40 / 48 / 56 jumps");
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
                if (!completed) AssetDatabase.DeleteAsset(ScenePath);
            }
        }

        // Each row is an authored eight-jump phrase: center distance, top height, landing width.
        // The eighth landing is deliberately wide; it is a breathing space, not a checkpoint.
        private static StageDefinition CreateEasy()
        {
            var map = Start();
            Add(map, "WARM UP", new[] {3.4f,3.6f,3.2f,3.8f,3.4f,4f,3.5f,3.8f},
                new[] {0f,0f,0.25f,0.25f,0f,0f,0.3f,0f}, new[] {3.2f,3f,3f,3.2f,2.8f,3f,3f,3.6f});
            Add(map, "GENTLE STEPS", new[] {3.6f,3.7f,3.6f,3.9f,3.6f,3.8f,3.5f,3.8f},
                new[] {0.3f,0.6f,0.9f,0.6f,0.2f,-0.2f,0f,0f}, new[] {3f,2.8f,2.8f,3f,2.8f,3f,2.8f,3.6f});
            Add(map, "VALLEY HOPS", new[] {3.6f,4.1f,3.5f,3.6f,4f,3.8f,3.6f,4f},
                new[] {-0.4f,-0.6f,-0.2f,0.3f,0.7f,0.1f,-0.3f,0f}, new[] {3f,2.8f,2.8f,2.8f,3f,2.8f,2.8f,3.6f});
            Add(map, "LONG AND SHORT", new[] {4.6f,3.2f,4.8f,3.4f,4.5f,3.5f,4.7f,4f},
                new[] {0f,0.2f,0.2f,-0.2f,0f,0.3f,0.1f,0f}, new[] {3f,2.6f,3f,2.8f,3f,2.8f,3f,3.6f});
            Add(map, "FIRST SUMMIT", new[] {3.8f,4.2f,3.5f,4.6f,3.4f,4.5f,3.8f,4.3f},
                new[] {0.4f,0.8f,0.2f,-0.3f,0f,0.5f,0.2f,0f}, new[] {2.8f,2.8f,2.6f,3f,2.6f,2.8f,2.8f,3.8f});
            return Save("Stage01_Easy", "STAGE 1 - SKY MEADOW", "EASY", new Color(0.14f,0.32f,0.45f), new Color(0.8f,0.96f,1f), map);
        }
        private static StageDefinition CreateNormal()
        {
            var map = Start();
            Add(map, "CHANGING RHYTHM", new[] {3.5f,4.5f,3.2f,4.8f,3.8f,4.4f,3.3f,4.2f},
                new[] {0.2f,0.2f,0.7f,0.1f,-0.3f,0.3f,0.5f,0f}, new[] {2.5f,2.4f,2.2f,2.5f,2.2f,2.4f,2.2f,3.2f});
            Add(map, "CLOUD STAIRS", new[] {3.5f,3.8f,3.6f,4.4f,3.8f,3.4f,4.1f,4f},
                new[] {0.5f,1f,1.4f,0.7f,0f,-0.6f,-0.2f,0f}, new[] {2.3f,2.2f,2f,2.2f,2.3f,2.2f,2.1f,3.2f});
            Add(map, "WIDE CROSSINGS", new[] {4.8f,4.9f,3.4f,5f,3.6f,4.8f,3.5f,4.6f},
                new[] {0f,-0.2f,0.3f,0.3f,0.7f,0.2f,-0.2f,0f}, new[] {2.4f,2.3f,2.1f,2.4f,2.1f,2.2f,2.1f,3.2f});
            Add(map, "SMALL ISLANDS", new[] {3.7f,4.1f,3.5f,4.3f,3.6f,4.2f,3.8f,4.1f},
                new[] {0.4f,0f,0.6f,0.2f,-0.4f,0.1f,0.5f,0f}, new[] {2f,1.9f,2f,1.8f,2f,1.9f,2f,3.2f});
            Add(map, "HIGH AND LOW", new[] {3.7f,4.6f,3.5f,4.8f,3.7f,4.4f,3.6f,4.2f},
                new[] {0.9f,-0.2f,0.8f,-0.5f,0.4f,-0.4f,0.5f,0f}, new[] {2.2f,2.2f,2.1f,2.3f,2f,2.1f,2f,3.2f});
            Add(map, "RIDGE FINALE", new[] {4.5f,3.5f,4.8f,3.6f,4.3f,3.8f,4.7f,4.4f},
                new[] {0.3f,1f,0.4f,-0.3f,0.4f,0.9f,0.2f,0f}, new[] {2.1f,1.9f,2.2f,2f,1.9f,2.1f,2f,3.6f});
            return Save("Stage02_Normal", "STAGE 2 - WIND RIDGE", "NORMAL", new Color(0.22f,0.22f,0.4f), new Color(0.8f,0.82f,1f), map);
        }
        private static StageDefinition CreateHard()
        {
            var map = Start();
            Add(map, "PRECISION", new[] {3.6f,4.2f,3.5f,4.6f,3.7f,4.3f,3.4f,4.2f},
                new[] {0.3f,0f,0.7f,0.2f,-0.5f,0.2f,0.5f,0f}, new[] {1.7f,1.6f,1.5f,1.6f,1.5f,1.4f,1.5f,2.8f});
            Add(map, "HIGH STEPS", new[] {3.6f,3.8f,3.5f,4.4f,3.7f,3.8f,4.2f,4.1f},
                new[] {0.8f,1.6f,0.5f,-0.7f,0.4f,1.3f,0.2f,0f}, new[] {1.7f,1.5f,1.5f,1.6f,1.5f,1.4f,1.5f,2.8f});
            Add(map, "WIDE GAPS", new[] {4.7f,3.5f,5.2f,3.6f,5f,3.7f,5.2f,4.5f},
                new[] {0f,0.5f,0.1f,-0.5f,-0.2f,0.6f,0.1f,0f}, new[] {1.7f,1.4f,1.6f,1.5f,1.6f,1.4f,1.5f,2.8f});
            Add(map, "DOWNHILL CONTROL", new[] {3.5f,4.3f,4.2f,3.8f,3.7f,4.4f,3.8f,4.2f},
                new[] {0.9f,-0.2f,-1.2f,-0.5f,0.6f,-0.6f,0.4f,0f}, new[] {1.5f,1.4f,1.3f,1.5f,1.4f,1.3f,1.5f,2.8f});
            Add(map, "RHYTHM REVERSALS", new[] {3.1f,4.9f,3.2f,5.1f,3.3f,4.8f,3.2f,4.5f},
                new[] {0.4f,0f,0.7f,0.1f,-0.4f,0f,0.5f,0f}, new[] {1.4f,1.6f,1.3f,1.5f,1.4f,1.4f,1.3f,2.8f});
            Add(map, "NEEDLE RIDGE", new[] {3.8f,4.2f,3.6f,4.4f,3.7f,4.3f,3.6f,4.2f},
                new[] {0.6f,0.1f,0.9f,0.2f,-0.6f,0.1f,0.8f,0f}, new[] {1.3f,1.2f,1.3f,1.1f,1.3f,1.2f,1.3f,2.8f});
            Add(map, "FINAL EXAM", new[] {4.5f,3.6f,4.7f,3.5f,5.1f,3.7f,4.5f,4.8f},
                new[] {0.2f,1.2f,0.1f,-0.8f,-0.4f,0.7f,0.2f,0f}, new[] {1.5f,1.3f,1.2f,1.4f,1.5f,1.2f,1.3f,3.4f});
            return Save("Stage03_Hard", "STAGE 3 - STORM SUMMIT", "HARD", new Color(0.18f,0.17f,0.25f), new Color(0.96f,0.76f,0.82f), map);
        }
        private static List<StagePlatformLayout> Start() => new List<StagePlatformLayout>
        { new StagePlatformLayout { position = new Vector2(0,-0.3f), width = 3.2f, section = "START" } };
        private static void Add(List<StagePlatformLayout> map, string section, float[] distances, float[] heights, float[] widths)
        {
            for (int i = 0; i < distances.Length; i++)
                map.Add(new StagePlatformLayout { position = new Vector2(map[map.Count-1].position.x + distances[i], heights[i] - 0.3f), width = widths[i], section = section });
        }
        private static StageDefinition Save(string file, string title, string difficulty, Color sky, Color color, List<StagePlatformLayout> map)
        {
            string path = DataFolder + "/" + file + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<StageDefinition>(path);
            if (existing != null) return existing;
            var data = ScriptableObject.CreateInstance<StageDefinition>();
            data.stageName = title; data.difficulty = difficulty; data.skyColor = sky;
            data.platformColor = color; data.platforms = map.ToArray();
            AssetDatabase.CreateAsset(data, path);
            return data;
        }
        private static void Set(Object target, string name, Object value)
        {
            var data = new SerializedObject(target);
            data.FindProperty(name).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetArray(Object target, string name, Object[] values)
        {
            var data = new SerializedObject(target);
            var array = data.FindProperty(name);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
