using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CloudHop.Editor
{
    public static class GimmickBuilder
    {
        public const string MainScene = "Assets/_Project/Scenes/CloudHopGimmicks.unity";
        public const string LabScene = "Assets/_Project/Scenes/CloudHopGimmickLab.unity";
        [MenuItem("Cloud Hop/Create Gimmick Scenes (only if missing)")]
        public static void Build()
        {
            if (Application.isBatchMode) EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            BuildScene(MainScene, false);
            BuildScene(LabScene, true);
            Debug.Log("GIMMICK_SCENES_CREATED");
        }
        private static void BuildScene(string path, bool lab)
        {
            if (File.Exists(path)) { Debug.Log("Keeping existing " + path); return; }
            var previous = SceneManager.GetActiveScene();
            if (!AssetDatabase.CopyAsset(StageBuilder.ScenePath, path)) throw new InvalidOperationException("Cannot copy base stage scene.");
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            bool completed = false;
            try
            {
                var roots = scene.GetRootGameObjects();
                var game = roots.SelectMany(x => x.GetComponentsInChildren<GameManager>(true)).Single();
                var director = game.GetComponent<StageDirector>();
                var refs = new SerializedObject(director).FindProperty("courses");
                var courses = Enumerable.Range(0, refs.arraySize).Select(i => (StageCourse)refs.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
                var clocks = new StageMechanics[courses.Length];
                var ui = roots.Single(x => x.name == "Prototype UI");
                var camera = roots.Single(x => x.name == "Main Camera").GetComponent<Camera>();
                camera.orthographicSize = 6.5f;
                Set(camera.GetComponent<FollowCamera>(), "fixedY", 2f);
                camera.transform.position = new Vector3(3, 2, -10);
                for (int s = 0; s < courses.Length; s++)
                {
                    var course = courses[s];
                    if (lab) MakeLabCourse(course, s);
                    var clock = course.gameObject.AddComponent<StageMechanics>();
                    Set(clock, "game", game);
                    clocks[s] = clock;
                    if (lab)
                    {
                        if (s == 0) { Bird(course, 2, 4.5f); Wind(course, 5); }
                        if (s == 1) { Crumble(course, 2, 2.2f); Moving(course, 5, 0.3f); }
                        if (s == 2) { Bolt(course, 2); Bolt(course, 6); }
                    }
                    else if (s == 0)
                        foreach (int n in new[] {13,21,29,37}) Bird(course, n, 4.5f);
                    else if (s == 1)
                    {
                        foreach (int n in new[] {5,19,33}) Bird(course, n, 4f);
                        Wind(course, 11); Wind(course, 27);
                        foreach (int n in new[] {15,23,39}) Crumble(course, n, 2.4f);
                        foreach (int n in new[] {35,43}) Moving(course, n, 0.25f);
                    }
                    else
                    {
                        foreach (int n in new[] {3,17,35,47}) Bird(course, n, 3.4f);
                        Wind(course, 11); Wind(course, 31); Wind(course, 53);
                        foreach (int n in new[] {13,27,43}) Crumble(course, n, 1.8f);
                        foreach (int n in new[] {19,45}) Moving(course, n, 0.2f);
                        foreach (int n in new[] {7,23,39,51}) Bolt(course, n);
                    }
                }
                var controls = ui.AddComponent<GimmickTestControls>();
                Set(controls, "game", game); SetArray(controls, "courses", clocks);
                var sampleButton = ui.transform.Find("Select Stage 1").GetComponent<Button>();
                var toggle = Object.Instantiate(sampleButton.gameObject, ui.transform).GetComponent<Button>();
                toggle.name = "Toggle Gimmicks";
                // The cloned stage button has a persistent SelectStage listener; remove it.
                var buttonData = new SerializedObject(toggle);
                buttonData.FindProperty("m_OnClick.m_PersistentCalls.m_Calls").ClearArray();
                buttonData.ApplyModifiedPropertiesWithoutUndo();
                toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);
                toggle.GetComponent<RectTransform>().sizeDelta = new Vector2(190,32);
                var toggleLabel = toggle.GetComponentInChildren<Text>();
                toggleLabel.text = "GIMMICKS: ON";
                toggleLabel.rectTransform.sizeDelta = new Vector2(190,32);
                Set(controls, "toggleButton", toggle); Set(controls, "label", toggleLabel);
                var legend = Object.Instantiate(ui.transform.Find("Course Hint").gameObject, ui.transform).GetComponent<Text>();
                legend.name = "Gimmick Legend";
                legend.rectTransform.anchoredPosition = new Vector2(0,185);
                legend.fontSize = 13;
                legend.supportRichText = true;
                legend.text = "<color=#ff59bf>BIRD</color>  |  <color=#33e6ff>WIND</color>  |  <color=#ffa633>CRUMBLE</color>  |  <color=#4dff66>MOVE</color>  |  <color=#ffe640>BOLT</color>";
                if (lab)
                {
                    string[] names = {"1 BIRD / WIND", "2 CLOUD / MOVE", "3 LIGHTNING"};
                    for (int i = 0; i < 3; i++) ui.transform.Find("Select Stage " + (i+1)).GetComponentInChildren<Text>().text = names[i];
                }
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                completed = true;
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
                if (!completed) AssetDatabase.DeleteAsset(path);
            }
        }
        private static void MakeLabCourse(StageCourse course, int index)
        {
            var platforms = course.Platforms.Take(9).ToArray();
            foreach (var extra in course.Platforms.Skip(9)) Object.DestroyImmediate(extra.gameObject);
            string folder = "Assets/_Project/ScriptableObjects/GimmickLab";
            Directory.CreateDirectory(folder); AssetDatabase.Refresh();
            string path = folder + "/Lab" + (index+1) + ".asset";
            var definition = AssetDatabase.LoadAssetAtPath<StageDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<StageDefinition>();
                definition.stageName = new[] {"LAB 1 - BIRD / WIND", "LAB 2 - CRUMBLE / MOVE", "LAB 3 - LIGHTNING"}[index];
                definition.difficulty = "TEST";
                definition.skyColor = course.Definition.skyColor;
                definition.platformColor = course.Definition.platformColor;
                definition.platforms = new StagePlatformLayout[9];
                for (int i = 0; i < 9; i++) definition.platforms[i] = new StagePlatformLayout { position = new Vector2(i*3.8f,-0.3f), width = 3.2f, section = "GIMMICK TEST" };
                AssetDatabase.CreateAsset(definition, path);
            }
            for (int i = 0; i < 9; i++)
            {
                platforms[i].transform.position = definition.platforms[i].position;
                platforms[i].transform.localScale = new Vector3(3.2f,0.6f,1);
                platforms[i].name = i == 8 ? "FINISH" : "Lab Platform " + i;
                platforms[i].GetComponent<SpriteRenderer>().color = i == 8 ? Color.yellow : definition.platformColor;
            }
            Set(course, "definition", definition); SetArray(course, "platforms", platforms);
        }
        private static Vector2 Gap(StageCourse course, int landing)
        {
            var a = course.Platforms[landing-1].transform;
            var b = course.Platforms[landing].transform;
            float right = a.position.x + a.localScale.x*0.5f;
            float left = b.position.x - b.localScale.x*0.5f;
            return new Vector2((right+left)*0.5f, Mathf.Max(a.position.y,b.position.y)+0.3f);
        }
        private static T Zone<T>(StageCourse course, int landing, Vector2 position, Vector2 size, Color color) where T : Gimmick
        {
            var obj = new GameObject(typeof(T).Name + " - Jump " + landing, typeof(BoxCollider2D));
            obj.transform.SetParent(course.transform);
            obj.transform.position = position;
            var collider = obj.GetComponent<BoxCollider2D>();
            collider.size = size; collider.isTrigger = true;
            var child = new GameObject("Placeholder", typeof(SpriteRenderer));
            child.transform.SetParent(obj.transform, false);
            child.transform.localScale = new Vector3(size.x,size.y,1);
            var sprite = child.GetComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/PrototypeSquare.png");
            sprite.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/PrototypeUnlit.mat");
            sprite.color = color;
            var gimmick = obj.AddComponent<T>();
            Label(gimmick, sprite, size.y*0.5f+0.2f);
            return gimmick;
        }
        private static void Bird(StageCourse course, int n, float period)
        {
            Vector2 gap = Gap(course,n);
            var bird = Zone<PatrolBird>(course,n,gap+Vector2.up*1.35f,new Vector2(0.7f,0.4f),new Color(1,0.35f,0.75f));
            Set(bird, "period", period); Set(bird, "halfRange", 1.1f);
            Kinematic(bird.GetComponent<Rigidbody2D>());
        }
        private static void Wind(StageCourse course, int n)
        {
            Vector2 gap = Gap(course,n);
            var wind = Zone<WindZone>(course,n,gap+Vector2.up*1.65f,new Vector2(2f,3.2f),new Color(0.2f,0.9f,1f,0.12f));
            Set(wind, "liftSpeed", 7f); Set(wind, "inactiveDuration", 5f);
            Set(wind, "activeDuration", 5f);
        }
        private static void Bolt(StageCourse course, int n)
        {
            Vector2 gap = Gap(course,n);
            var bolt = Zone<LightningZone>(course,n,new Vector2(gap.x,1.5f),new Vector2(0.5f,5.5f),new Color(1,0.9f,0.25f,0.12f));
            Set(bolt, "activeDuration", 0.4f);
        }
        private static void Crumble(StageCourse course, int n, float delay)
        {
            var platform = course.Platforms[n];
            var crumble = platform.gameObject.AddComponent<CrumblingPlatform>();
            Set(crumble, "crumbleDelay", delay);
            Label(crumble, platform.GetComponent<SpriteRenderer>(), 0.55f);
        }
        private static void Moving(StageCourse course, int n, float amplitude)
        {
            var platform = course.Platforms[n];
            var moving = platform.gameObject.AddComponent<MovingPlatform>();
            Kinematic(moving.GetComponent<Rigidbody2D>());
            Set(moving, "amplitude", amplitude);
            Label(moving, platform.GetComponent<SpriteRenderer>(), 0.55f);
        }
        private static void Kinematic(Rigidbody2D body)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        private static void Label(Gimmick gimmick, SpriteRenderer visual, float offset)
        {
            var obj = new GameObject("Debug Label", typeof(TextMesh));
            obj.transform.SetParent(gimmick.transform, false);
            Vector3 scale = gimmick.transform.lossyScale;
            obj.transform.localScale = new Vector3(1/scale.x,1/scale.y,1);
            obj.transform.position = gimmick.transform.position + Vector3.up*offset;
            var text = obj.GetComponent<TextMesh>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 40; text.characterSize = 0.12f;
            text.anchor = TextAnchor.LowerCenter; text.alignment = TextAlignment.Center;
            text.text = gimmick.GetType().Name;
            var renderer = obj.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = text.font.material; renderer.sortingOrder = 30;
            visual.color = gimmick is MovingPlatform ? new Color(0.3f,1,0.4f) : visual.color;
            Set(gimmick, "visual", visual); Set(gimmick, "debugLabel", text);
        }
        private static void Set(Object target, string name, Object value)
        {
            var data = new SerializedObject(target); data.FindProperty(name).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Set(Object target, string name, float value)
        {
            var data = new SerializedObject(target); data.FindProperty(name).floatValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetArray(Object target, string name, Object[] values)
        {
            var data = new SerializedObject(target); var array = data.FindProperty(name); array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
