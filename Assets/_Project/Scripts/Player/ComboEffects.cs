using UnityEngine;

namespace CloudHop
{
    public sealed class ComboEffects : MonoBehaviour
    {
        private PlayerController player;
        private AudioSource sound;
        private AudioClip chime, rush;
        private TrailRenderer trail;
        private Material material;
        private void Awake()
        {
            player = GetComponent<PlayerController>();
            sound = gameObject.AddComponent<AudioSource>();
            sound.playOnAwake = false; sound.spatialBlend = 0; sound.volume = .2f;
            chime = Tone(false); rush = Tone(true);
            trail = new GameObject("Boost trail").AddComponent<TrailRenderer>();
            trail.transform.SetParent(transform, false);
            trail.transform.localPosition = new Vector3(0, .1f, 0);
            material = new Material(Shader.Find("Sprites/Default"));
            trail.sharedMaterial = material;
            trail.time = .18f; trail.minVertexDistance = .05f;
            trail.startWidth = .3f; trail.endWidth = 0;
            trail.startColor = new Color(.25f, .9f, 1, .8f); trail.endColor = new Color(.8f, 1, 1, 0);
            trail.sortingOrder = 8; trail.emitting = false;
        }
        private void OnEnable()
        {
            player.ComboLanded += Landed; player.BoostUsed += Boost; player.ResetPerformed += ResetEffects;
        }
        private void OnDisable()
        {
            player.ComboLanded -= Landed; player.BoostUsed -= Boost; player.ResetPerformed -= ResetEffects;
            ResetEffects();
        }
        private void Update() => trail.emitting = player.IsBoosting && Time.timeScale > 0;
        private void Landed(ComboLanding result)
        {
            if (result != ComboLanding.Perfect) return;
            sound.pitch = 1 + Mathf.Min(player.Combo.Count - 1, 5) * .08f;
            sound.PlayOneShot(chime);
        }
        private void Boost() { sound.pitch = 1; sound.PlayOneShot(rush); }
        private void ResetEffects() { trail.Clear(); trail.emitting = false; sound.Stop(); }
        private static AudioClip Tone(bool wind)
        {
            const int rate = 22050;
            var samples = new float[(int)(rate * (wind ? .24f : .13f))];
            uint noise = 391;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate, envelope = Mathf.Sin(Mathf.PI * i / samples.Length);
                noise = noise * 1664525 + 1013904223;
                samples[i] = envelope * (wind ? ((noise & 65535) / 32767.5f - 1) * .23f : Mathf.Sin(2 * Mathf.PI * 880 * t) * .35f);
            }
            var clip = AudioClip.Create(wind ? "Combo wind" : "Perfect chime", samples.Length, 1, rate, false);
            clip.SetData(samples, 0); return clip;
        }
        private void OnDestroy() { Destroy(chime); Destroy(rush); Destroy(material); }
    }
}
