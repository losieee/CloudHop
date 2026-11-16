using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace CloudHop.Editor
{
    [InitializeOnLoad]
    public static class CastleCheckRunner
    {
        static readonly TestRunnerApi Api;
        static CastleCheckRunner()
        {
            Api = ScriptableObject.CreateInstance<TestRunnerApi>();
            Api.RegisterCallbacks(new Results());
        }
        [MenuItem("Cloud Hop/Check Castle Changes")]
        public static void Run()
        {
            SessionState.SetBool("CastleCheck.Running", true);
            Api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode,
                testNames = new[] { "CloudHop.Tests.CastleGalleryTests", "CloudHop.Tests.PlatformEdgeTests" } }));
        }
        private sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                if (!SessionState.GetBool("CastleCheck.Running", false)) return;
                SessionState.SetBool("CastleCheck.Running", false);
                System.IO.Directory.CreateDirectory("Logs");
                TestRunnerApi.SaveResultToFile(result, "Logs/castle-checks.xml");
                Debug.Log($"CASTLE_CHECKS_DONE: passed={result.PassCount}, failed={result.FailCount}");
            }
        }
    }
}
