using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CloudHop.Tests
{
    public sealed class CharacterPoseTests
    {
        [UnityTest]
        public IEnumerator AdditionalSkinsFollowPhysicsAndResetWithoutChangingCollider()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return RunPoses();
        }

        private static IEnumerator RunPoses()
        {
            Time.timeScale = 1;
            var floor = new GameObject("Animation test platform");
            floor.AddComponent<BoxCollider2D>().size = new Vector2(100, .2f);
            floor.AddComponent<Platform>();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Player.prefab");
            var player = Object.Instantiate(prefab).GetComponent<PlayerController>();
            player.GetComponent<ChargeInput>().enabled = false;
            var visual = player.GetComponentInChildren<CharacterVisual>();
            var renderer = visual.GetComponent<SpriteRenderer>();
            var collider = player.GetComponent<BoxCollider2D>();
            var size = collider.size;
            var body = player.GetComponent<Rigidbody2D>();
            foreach (string name in new[] { "Girl", "Fox" })
            {
                var skin = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/ScriptableObjects/" + name + "Character.asset");
                var poses = new[] { skin.sprite, skin.chargeSprite, skin.jumpSprite, skin.landingSprite, skin.fallSprite };
                foreach (var pose in poses) Assert.IsNotNull(pose, name + " is missing a pose");
                Assert.AreEqual(5, new System.Collections.Generic.HashSet<Sprite>(poses).Count);
                visual.Apply(skin);
                player.ResetPlayer(new Vector3(0, .6f, 0));
                yield return WaitFor(() => player.IsGrounded && renderer.sprite == skin.sprite, name + " spawn");
                Assert.IsTrue(player.IsGrounded);
                Assert.AreEqual(skin.sprite, renderer.sprite, "Spawn must show idle");
                Capture(name + "-idle", renderer);
                player.BeginCharge();
                yield return new WaitForSeconds(.12f);
                Assert.AreEqual(skin.chargeSprite, renderer.sprite);
                Capture(name + "-charge", renderer);
                player.ReleaseCharge();
                yield return new WaitForSeconds(.08f);
                Assert.Greater(body.linearVelocity.y, 0);
                Assert.AreEqual(skin.jumpSprite, renderer.sprite);
                Capture(name + "-jump", renderer);
                float deadline = Time.realtimeSinceStartup + 3;
                while (renderer.sprite != skin.fallSprite && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(skin.fallSprite, renderer.sprite);
                Capture(name + "-fall", renderer);
                while (renderer.sprite != skin.landingSprite && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(skin.landingSprite, renderer.sprite);
                Capture(name + "-landing", renderer);
                yield return WaitFor(() => renderer.sprite == skin.sprite, name + " landing recovery");
                Assert.AreEqual(skin.sprite, renderer.sprite, "Landing must return to idle");
                // A second jump proves landing recovery cannot block the next charge.
                player.BeginCharge();
                yield return new WaitForSeconds(.08f);
                Assert.AreEqual(skin.chargeSprite, renderer.sprite);
                player.ReleaseCharge();
                yield return new WaitForSeconds(.08f);
                while (renderer.sprite != skin.landingSprite && Time.realtimeSinceStartup < deadline + 3) yield return null;
                Assert.AreEqual(skin.landingSprite, renderer.sprite);
                player.BeginCharge();
                yield return null; yield return null;
                Assert.AreEqual(skin.chargeSprite, renderer.sprite, "Charge must override landing immediately");
                player.ResetPlayer(new Vector3(0, .6f, 0));
                Assert.AreEqual(skin.sprite, renderer.sprite, "Retry clears the previous pose");
                yield return WaitFor(() => player.IsGrounded && renderer.sprite == skin.sprite, name + " retry");
                Assert.AreEqual(skin.sprite, renderer.sprite);
                Assert.AreEqual(size, collider.size);
                Assert.AreEqual(Vector3.one, player.transform.localScale);
            }
        }

        private static IEnumerator WaitFor(System.Func<bool> condition, string step)
        {
            float deadline = Time.realtimeSinceStartup + 4;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(condition(), step);
        }
        private static void Capture(string name, SpriteRenderer renderer)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            var cameraObject = new GameObject("Pose preview camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = .85f;
            camera.transform.position = renderer.bounds.center + Vector3.back * 10;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.27f, .65f, .9f);
            var target = new RenderTexture(512, 512, 24);
            target.Create(); camera.targetTexture = target; camera.Render();
            var previous = RenderTexture.active; RenderTexture.active = target;
            var image = new Texture2D(512, 512, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 512, 512), 0, 0); image.Apply();
            Directory.CreateDirectory("CharacterPreview");
            File.WriteAllBytes("CharacterPreview/" + name + ".png", image.EncodeToPNG());
            RenderTexture.active = previous; camera.targetTexture = null; target.Release();
            Object.Destroy(cameraObject); Object.Destroy(target); Object.Destroy(image);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
            Time.timeScale = 1;
        }
    }
}
