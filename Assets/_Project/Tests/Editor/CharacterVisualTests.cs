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
    public sealed class CharacterVisualTests
    {
        [UnityTest]
        public IEnumerator UploadedPosesFollowPlayerWithoutChangingPhysics()
        {
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/CloudHopGimmickLab.unity");
            yield return new EnterPlayMode();
            var player = Object.FindFirstObjectByType<PlayerController>();
            player.GetComponent<ChargeInput>().enabled = false;
            var body = player.GetComponent<Rigidbody2D>();
            var renderer = player.GetComponentInChildren<SpriteRenderer>();
            var skin = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/ScriptableObjects/CloudAdventurer.asset");
                        float deadline = Time.time + 3;
            while (!player.IsGrounded && Time.time < deadline) yield return null;
            Assert.IsTrue(player.IsGrounded, "Player must reach the starting platform.");
            yield return null;
            yield return null;
            Assert.AreEqual(new Vector2(0.6f,0.9f), player.GetComponent<BoxCollider2D>().size);
            Assert.AreEqual(2,body.gravityScale);
            Assert.AreEqual(skin.sprite,renderer.sprite);
            Capture("idle");
            player.BeginCharge();
            yield return null;
            yield return null;
            Assert.AreEqual(skin.chargeSprite,renderer.sprite);
            Capture("charge");
            player.ReleaseCharge();
            yield return new WaitForFixedUpdate();
            yield return null;
            yield return null;
            Assert.AreEqual(skin.jumpSprite,renderer.sprite);
            Capture("jump");
            body.linearVelocity = new Vector2(5,-2);
            yield return null;
            yield return null;
            Assert.AreEqual(skin.fallSprite,renderer.sprite);
            Capture("fall");
            Object.FindFirstObjectByType<GameManager>().Retry();
                        float retryDeadline = Time.time + 3;
            while (!player.IsGrounded && Time.time < retryDeadline) yield return null;
            Assert.IsTrue(player.IsGrounded);
            yield return null;
            yield return null;
            Assert.AreEqual(skin.sprite,renderer.sprite);
        }

        [UnityTearDown]
        public IEnumerator Restore()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        private static void Capture(string pose)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            var camera = Camera.main;
            var target = new RenderTexture(1280,800,24);
            target.Create();
            camera.targetTexture = target;
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(1280,800,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1280,800),0,0);
            texture.Apply();
            Directory.CreateDirectory("CharacterPreview");
            File.WriteAllBytes("CharacterPreview/" + pose + ".png",texture.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            target.Release();
            Object.Destroy(texture);
            Object.Destroy(target);
        }
    }
}