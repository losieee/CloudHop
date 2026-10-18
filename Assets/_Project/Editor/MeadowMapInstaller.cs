using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace CloudHop.Editor
{
    public static class MeadowMapInstaller
    {
        [MenuItem("Cloud Hop/맵/공통 1스테이지 높낮이와 코인 적용")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(PlatformBuilds.SharedScene);
            var course = Object.FindObjectsByType<StageCourse>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single(c => c.Definition.difficulty == "EASY");
            var data = course.Definition;
            float[] tops = { 0, .9f, 1.8f, .2f, 0, .7f, 1.4f, .7f, 0 };
            for (int n = 0; n < tops.Length; n++)
            {
                int i = n + 8;
                var layout = data.platforms[i]; layout.position.y = tops[n] - .3f;
                layout.section = i <= 10 ? "CLOUD ASCENT" : i <= 13 ? "LOWER GARDEN" : "TRAIL REJOIN";
                data.platforms[i] = layout;
                course.Platforms[i].transform.position = layout.position;
            }
            // Keep the first branching lesson free of the old bird's patrol.
            var bird = course.GetComponentsInChildren<PatrolBird>(true).FirstOrDefault(x => x.name == "PatrolBird - Jump 13" || x.name == "PatrolBird - Jump 18");
            if (bird != null)
            {
                bird.name = "PatrolBird - Jump 18";
                bird.transform.position = new Vector3((data.platforms[17].position.x + data.platforms[18].position.x) * .5f, 1.8f, 0);
            }
            data.followElevation = true;
            var root = course.transform.Find("Upper Cloud Route");
            if (root == null) { root = new GameObject("Upper Cloud Route").transform; root.SetParent(course.transform, false); }
            var a = Branch(root, "Upper Landing A", new Vector2(39.9f, 3.4f), 2.2f, 11);
            var b = Branch(root, "Upper Landing B", new Vector2(45.1f, 4f), 2.4f, 13);
            if (course.GetComponent<StageCollectibles>() == null) course.gameObject.AddComponent<StageCollectibles>();
            var coins = new List<StageCoinLayout>();
            // Coin curves match ballistic centre-to-centre jumps at the shared horizontal speed.
            for (int i = 1; i <= 17; i++) Trail(coins, data.platforms[i-1].position + Vector2.up * .78f, data.platforms[i].position + Vector2.up * .78f, false);
            Trail(coins, data.platforms[10].position + Vector2.up * .78f, (Vector2)a.position + Vector2.up * .78f, true);
            Trail(coins, (Vector2)a.position + Vector2.up * .78f, (Vector2)b.position + Vector2.up * .78f, true);
            Trail(coins, (Vector2)b.position + Vector2.up * .78f, data.platforms[14].position + Vector2.up * .78f, true);
            data.coins = coins.ToArray();
            EditorUtility.SetDirty(data); EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("MEADOW_MAP_INSTALLED: " + coins.Count + " coins, shared PC/Android scene");
        }
        internal static Transform Branch(Transform parent, string name, Vector2 position, float width, int progress)
        {
            var existing = parent.Find(name);
            var obj = existing != null ? existing.gameObject : (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Platforms/Platform.prefab"), parent);
            obj.name = name; obj.transform.position = position; obj.transform.localScale = new Vector3(width, .6f, 1);
            var branch = obj.GetComponent<BranchLanding>(); if (branch == null) branch = obj.AddComponent<BranchLanding>(); branch.Configure(progress);
            if (obj.GetComponent<PerfectLandingZone>() == null) obj.AddComponent<PerfectLandingZone>();
            var effector = obj.GetComponent<PlatformEffector2D>(); if (effector == null) effector = obj.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true; effector.surfaceArc = 160;
            obj.GetComponent<BoxCollider2D>().usedByEffector = true;
            return obj.transform;
        }
        internal static void Trail(List<StageCoinLayout> coins, Vector2 start, Vector2 finish, bool golden)
        {
            float duration = (finish.x - start.x) / 5f;
            float vy = (finish.y - start.y + .5f * 19.62f * duration * duration) / duration;
            for (int i = 1; i <= 4; i++)
            {
                float time = duration * i / 5f;
                coins.Add(new StageCoinLayout { position = start + new Vector2(5 * time, vy * time - .5f * 19.62f * time * time), value = golden && i == 3 ? 5 : 1 });
            }
        }
    }
}
