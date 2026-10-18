using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace CloudHop.Editor
{
    public static class ChoiceRouteInstaller
    {
        [MenuItem("Cloud Hop/Install Stage Choice Routes")]
        public static void Install()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHop.unity");
            var courses = Object.FindObjectsByType<StageCourse>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var course in courses)
            {
                var data = course.Definition;
                if (data.difficulty == "EASY") data.choiceRoutes = new[] { Route(3, 2) };
                if (data.difficulty == "NORMAL")
                {
                    data.choiceRoutes = new[] { Route(8, 2.4f), Route(24, 2.4f) };
                    SetLanding(course, 9, -.7f, 3.6f);
                    SetLanding(course, 25, -.7f, 3.6f);
                }
                if (data.difficulty == "HARD")
                {
                    data.choiceRoutes = new[] { Route(10, 3.5f), Route(19, 2.7f) };
                    SetLanding(course, 11, 0, 2.4f);
                    SetLanding(course, 12, 1.5f, 2.6f);
                    SetLanding(course, 13, .8f, 1.8f);
                    SetLanding(course, 20, -.7f, 2.8f);
                    SetLanding(course, 21, -.2f, 2.1f, data.platforms[19].position.x + 7.8f);
                    // Bring the existing lightning challenge into the moving-platform shortcut.
                    var bolt = course.GetComponentsInChildren<LightningZone>(true).Single(x => x.name == "LightningZone - Jump 23");
                    bolt.transform.position = new Vector3(data.platforms[19].position.x + 5.8f, 1.5f, 0);
                }
                EditorUtility.SetDirty(data);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("CHOICE_ROUTES_INSTALLED: stage 1 = 1, stages 2 and 3 = 2 each");
        }
        private static StageChoiceRoute Route(int launch, float height) => new StageChoiceRoute
        { launchIndex = launch, safeIndex = launch + 1, rejoinIndex = launch + 2, arcHeight = height };
        private static void SetLanding(StageCourse course, int index, float top, float width, float? x = null)
        {
            var layout = course.Definition.platforms[index];
            layout.position.y = top - .3f;
            if (x.HasValue) layout.position.x = x.Value;
            layout.width = width;
            course.Definition.platforms[index] = layout;
            var platform = course.Platforms[index];
            platform.transform.position = layout.position;
            platform.transform.localScale = new Vector3(width, .6f, 1);
        }
    }
}


