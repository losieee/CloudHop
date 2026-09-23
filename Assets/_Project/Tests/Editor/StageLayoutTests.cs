using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CloudHop.Tests
{
    public sealed class StageLayoutTests
    {
        [Test]
        public void FullFootTakeoffPositionsHaveAReachableLandingWindow()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Player.prefab");
            var values = new SerializedObject(prefab.GetComponent<PlayerController>());
            float min = values.FindProperty("minimumJumpSpeed").floatValue;
            float max = values.FindProperty("maximumJumpSpeed").floatValue;
            float speed = values.FindProperty("horizontalSpeed").floatValue;
            float gravity = -Physics2D.gravity.y * prefab.GetComponent<Rigidbody2D>().gravityScale;
            float halfPlayer = prefab.GetComponent<BoxCollider2D>().size.x * 0.5f;
            var failures = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:StageDefinition", new[] { "Assets/_Project/ScriptableObjects/Stages" });
            Assert.AreEqual(3, guids.Length, "All three stage data assets must be loaded.");
            foreach (string guid in guids)
            {
                var stage = AssetDatabase.LoadAssetAtPath<StageDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                for (int i = 1; i < stage.platforms.Length; i++)
                {
                    var from = stage.platforms[i-1];
                    var to = stage.platforms[i];
                    float dy = to.position.y - from.position.y;
                    Assert.Greater(to.position.x - from.position.x - (from.width + to.width) * 0.5f, 0, "Platforms must have a visible gap.");
                    foreach (float offset in new[] { -from.width * 0.5f + halfPlayer, 0f, from.width * 0.5f - halfPlayer })
                    {
                        bool reachable = false;
                        for (float launch = min; launch <= max + 0.001f; launch += 0.01f)
                        {
                            float effective = launch - gravity * Time.fixedDeltaTime * 0.5f;
                            float discriminant = effective * effective - 2 * gravity * dy;
                            if (discriminant < 0) continue;
                            float time = (effective + Mathf.Sqrt(discriminant)) / gravity;
                            float landing = from.position.x + offset + speed * time;
                            if (landing >= to.position.x - to.width * 0.5f + halfPlayer && landing <= to.position.x + to.width * 0.5f - halfPlayer)
                            { reachable = true; break; }
                        }
                        if (!reachable) failures.Add($"{stage.name}: jump {i}, takeoff offset {offset:F2}, dx {to.position.x-from.position.x:F2}, dy {dy:F2}");
                    }
                }
            }
            Assert.IsEmpty(failures, "Unreachable full-foot takeoffs:\n" + string.Join("\n", failures));
        }
    }
}
