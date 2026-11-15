using System.Collections.Generic;
using UnityEngine;

namespace CloudHop
{
    public sealed class CloudAudio : MonoBehaviour
    {
        private static CloudAudio current;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private AudioSource music, effects;

        private void Awake()
        {
            current = this;
            music = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.loop = true;
            music.volume = .23f;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
        }

        private void OnDestroy()
        {
            if (current == this) current = null;
        }

        private AudioClip Load(string key)
        {
            if (!clips.TryGetValue(key, out var clip))
                clips[key] = clip = Resources.Load<AudioClip>("CloudAudio/" + key);
            return clip;
        }

        public static void Theme(string key)
        {
            if (!current) return;
            var clip = current.Load(key);
            if (!clip || current.music.clip == clip && current.music.isPlaying) return;
            current.music.clip = clip;
            current.music.Play();
        }

        public static void Cue(string key, float volume)
        {
            if (!current) return;
            var clip = current.Load(key);
            if (clip) current.effects.PlayOneShot(clip, volume);
        }
    }
}
