using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace CloudHop.Editor
{
    public static class RidgeMapInstaller
    {
        [MenuItem("Cloud Hop/맵/공통 2스테이지 바람 능선 적용")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(PlatformBuilds.SharedScene);
            var course = Object.FindObjectsByType<StageCourse>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(c => c.Definition.difficulty == "NORMAL");
            var data = course.Definition;
            // Preserve the two existing boost choices and their wind-assisted exits.
            Heights(course, 12, new[] { .8f, 1.5f, 2.1f, 1.5f, 1f }, "RIDGE ASCENT");
            Heights(course, 17, new[] { .3f, -.4f, .4f, 1.2f, .6f, -.2f, .5f }, "LOWER WIND VALLEY");
            Heights(course, 28, new[] { .9f, 1.4f, 1.9f, 1.1f, .4f }, "FLOATING STEPS");
            Heights(course, 33, new[] { 1.1f, .6f, 1.3f, .5f, 1.1f, .5f, 1.2f, .6f }, "SWAYING RIDGE");
            Heights(course, 41, new[] { 1.2f, 1.9f, 2.2f, 2.8f, 2.2f, 1.5f, .7f, 0f }, "SUMMIT DESCENT");
            data.followElevation = true;
            var root = course.transform.Find("Golden Ridge Route");
            if (root == null) { root = new GameObject("Golden Ridge Route").transform; root.SetParent(course.transform, false); }
            var a = MeadowMapInstaller.Branch(root, "Ridge Landing A", new Vector2(66f, 3.2f), 2.2f, 17);
            var b = MeadowMapInstaller.Branch(root, "Ridge Landing B", new Vector2(71f, 3.2f), 2.1f, 18);
            var c = MeadowMapInstaller.Branch(root, "Ridge Landing C", new Vector2(76f, 2.5f), 2.2f, 19);
            // Relocate the existing hazards with their terrain, without adding unavoidable hazards.
            foreach (var bird in course.GetComponentsInChildren<PatrolBird>(true))
            {
                int n = int.Parse(bird.name.Substring(bird.name.LastIndexOf(' ') + 1));
                var left = data.platforms[n-1]; var right = data.platforms[n];
                bird.transform.position = new Vector3((left.position.x + right.position.x) * .5f,
                    Mathf.Max(left.position.y, right.position.y) + 1.65f, 0);
            }
            foreach (var moving in course.GetComponentsInChildren<MovingPlatform>(true))
            {
                var settings = new SerializedObject(moving);
                settings.FindProperty("amplitude").floatValue = .4f;
                settings.FindProperty("period").floatValue = 4.8f;
                settings.ApplyModifiedPropertiesWithoutUndo();
            }
            if (course.GetComponent<StageCollectibles>() == null) course.gameObject.AddComponent<StageCollectibles>();
            var coins = new List<StageCoinLayout>();
            // Avoid suggesting fixed flight paths through timed wind, birds and moving landings.
            int[] trails = { 1,2,3,7,12,13,14,15,16,17,18,20,21,22,23,28,29,30,31,32,37,38,39,40,41,42,45,46,47,48 };
            foreach (int i in trails) MeadowMapInstaller.Trail(coins,
                data.platforms[i-1].position + Vector2.up * .78f,
                data.platforms[i].position + Vector2.up * .78f, false);
            Vector2[] upper = { data.platforms[16].position, a.position, b.position, c.position, data.platforms[20].position };
            for (int i=1;i<upper.Length;i++) MeadowMapInstaller.Trail(coins,
                upper[i-1] + Vector2.up * .78f, upper[i] + Vector2.up * .78f, true);
            data.coins = coins.ToArray();
            EditorUtility.SetDirty(data); EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("RIDGE_MAP_INSTALLED: " + coins.Count + " coins, shared PC/Android");
        }
        private static void Heights(StageCourse course, int first, float[] tops, string section)
        {
            for (int n=0;n<tops.Length;n++)
            {
                int i=first+n; var layout=course.Definition.platforms[i];
                layout.position.y=tops[n]-.3f; layout.section=section;
                course.Definition.platforms[i]=layout; course.Platforms[i].transform.position=layout.position;
            }
        }
    }
}
