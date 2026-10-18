using UnityEngine;
using UnityEngine.SceneManagement;

namespace CloudHop
{
    public enum GamePlatform { PC, Android }

    // Entry scenes select presentation/input only; gameplay and saves stay shared.
    public sealed class PlatformEntry : MonoBehaviour
    {
        [SerializeField] private GamePlatform platform;
        private static GamePlatform? selected;
        public static bool IsMobile => (selected ?? (Application.platform == RuntimePlatform.Android ? GamePlatform.Android : GamePlatform.PC)) == GamePlatform.Android;
        public void Configure(GamePlatform value) => platform = value;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetMode() => selected = null;
        private void Awake()
        {
            selected = platform;
            if (IsMobile)
            {
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.orientation = ScreenOrientation.AutoRotation;
                Application.targetFrameRate = 60;
            }
            SceneManager.LoadSceneAsync("CloudHop", LoadSceneMode.Single);
        }
    }
}
