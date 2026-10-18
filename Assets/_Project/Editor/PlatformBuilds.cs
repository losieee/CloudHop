using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CloudHop.Editor
{
    public static class PlatformBuilds
    {
        public const string SharedScene = "Assets/_Project/Scenes/CloudHop.unity";
        public const string PCScene = "Assets/_Project/Scenes/CloudHop_PC.unity";
        public const string AndroidScene = "Assets/_Project/Scenes/CloudHop_Android.unity";

        [MenuItem("Cloud Hop/플랫폼/시작 씬 준비")]
        public static void Prepare()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            {
                if (SceneManager.GetActiveScene().isDirty)
                {
                    if (Application.isBatchMode) throw new InvalidOperationException("Save the current scene before preparing platform entries.");
                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                }
                EditorSceneManager.OpenScene(SharedScene);
            }
            CreateEntry(PCScene, GamePlatform.PC);
            CreateEntry(AndroidScene, GamePlatform.Android);
            // Both entry scenes are available for Editor preview. Each build includes only its own entry.
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene(PCScene, true),
                new EditorBuildSettingsScene(SharedScene, true),
                new EditorBuildSettingsScene(AndroidScene, true)
            };
            UITextExcelImporter.Import();
            AssetDatabase.SaveAssets();
        }
        private static void CreateEntry(string path, GamePlatform platform)
        {
            if (File.Exists(path)) return;
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var entry = new GameObject("Cloud Hop " + platform + " Entry").AddComponent<PlatformEntry>();
            SceneManager.MoveGameObjectToScene(entry.gameObject, scene);
            entry.Configure(platform);
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);
            if (previous.IsValid()) SceneManager.SetActiveScene(previous);
        }
        [MenuItem("Cloud Hop/플랫폼/PC 시작 씬 열기")]
        public static void OpenPC() => Open(PCScene);
        [MenuItem("Cloud Hop/플랫폼/Android 시작 씬 열기")]
        public static void OpenAndroid() => Open(AndroidScene);
        private static void Open(string scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Prepare();
            EditorSceneManager.OpenScene(scene);
        }
        public static string[] ScenesFor(GamePlatform platform) => new[] { platform == GamePlatform.Android ? AndroidScene : PCScene, SharedScene };
        [MenuItem("Cloud Hop/빌드/PC Windows 빌드")]
        public static void BuildPC() => Build(GamePlatform.PC);
        [MenuItem("Cloud Hop/빌드/Android APK 빌드")]
        public static void BuildAndroid() => Build(GamePlatform.Android);
        private static void Build(GamePlatform platform)
        {
            var target = platform == GamePlatform.Android ? BuildTarget.Android : BuildTarget.StandaloneWindows64;
            var group = platform == GamePlatform.Android ? BuildTargetGroup.Android : BuildTargetGroup.Standalone;
            if (!BuildPipeline.IsBuildTargetSupported(group, target))
                throw new BuildFailedException("Unity Hub에서 이 에디터의 " + (platform == GamePlatform.Android ? "Android Build Support (SDK/NDK/OpenJDK 포함)" : "Windows Build Support") + " 모듈을 설치하세요.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Prepare();
            var oldOrientation = PlayerSettings.defaultInterfaceOrientation;
            bool oldPortrait = PlayerSettings.allowedAutorotateToPortrait;
            bool oldUpsideDown = PlayerSettings.allowedAutorotateToPortraitUpsideDown;
            bool oldLeft = PlayerSettings.allowedAutorotateToLandscapeLeft;
            bool oldRight = PlayerSettings.allowedAutorotateToLandscapeRight;
            bool oldBundle = EditorUserBuildSettings.buildAppBundle;
            var oldArchitectures = PlayerSettings.Android.targetArchitectures;
            string oldIdentifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            var oldBackend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android);
            try
            {
                if (platform == GamePlatform.Android)
                {
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.cloudhop.game");
                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
                    PlayerSettings.allowedAutorotateToPortrait = false;
                    PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
                    PlayerSettings.allowedAutorotateToLandscapeLeft = true;
                    PlayerSettings.allowedAutorotateToLandscapeRight = true;
                    PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                    EditorUserBuildSettings.buildAppBundle = false;
                }
                string output = platform == GamePlatform.Android ? "Builds/Android/CloudHop.apk" : "Builds/PC/CloudHop.exe";
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = ScenesFor(platform), locationPathName = output, target = target, options = BuildOptions.None });
                if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("빌드 실패: " + report.summary.result);
                Debug.Log("Cloud Hop " + platform + " 빌드 완료: " + Path.GetFullPath(output));
            }
            finally
            {
                if (platform == GamePlatform.Android)
                {
                PlayerSettings.defaultInterfaceOrientation = oldOrientation;
                PlayerSettings.allowedAutorotateToPortrait = oldPortrait;
                PlayerSettings.allowedAutorotateToPortraitUpsideDown = oldUpsideDown;
                PlayerSettings.allowedAutorotateToLandscapeLeft = oldLeft;
                PlayerSettings.allowedAutorotateToLandscapeRight = oldRight;
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, oldIdentifier);
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, oldBackend);
                PlayerSettings.Android.targetArchitectures = oldArchitectures;
                EditorUserBuildSettings.buildAppBundle = oldBundle;
                }
            }
        }
    }
}
