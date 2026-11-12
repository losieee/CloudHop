using System.Collections.Generic;
using UnityEngine;

namespace CloudHop
{
    internal static class PCResolution
    {
        private const string Key = "CloudHop.Display.Size";
        internal static Vector2Int Desktop
        {
            get
            {
                var display = Screen.mainWindowDisplayInfo;
                if (display.width > 0 && display.height > 0)
                    return new Vector2Int(display.width, display.height);
                var mode = Screen.currentResolution;
                return new Vector2Int(mode.width, mode.height);
            }
        }

        internal static List<Vector2Int> Options()
        {
            var desktop = Desktop;
            var sizes = new List<Vector2Int> { Vector2Int.zero };
            foreach (var mode in Screen.resolutions)
            {
                var size = new Vector2Int(mode.width, mode.height);
                if (size.x >= 800 && size.y >= 600 && size.x <= desktop.x &&
                    size.y <= desktop.y && !sizes.Contains(size)) sizes.Add(size);
            }
            return sizes;
        }

        internal static int Selected(List<Vector2Int> sizes)
        {
            string saved = PlayerPrefs.GetString(Key, "");
            return Mathf.Max(0, sizes.FindIndex(s => s.x + "x" + s.y == saved));
        }

        internal static void Apply(Vector2Int size, bool save)
        {
            if (save)
            {
                if (size == Vector2Int.zero) PlayerPrefs.DeleteKey(Key);
                else PlayerPrefs.SetString(Key, size.x + "x" + size.y);
                PlayerPrefs.Save();
            }
            if (size == Vector2Int.zero) size = Desktop;
            if (!Application.isEditor && size.x > 0 && size.y > 0)
                Screen.SetResolution(size.x, size.y, FullScreenMode.FullScreenWindow);
        }
    }
}
