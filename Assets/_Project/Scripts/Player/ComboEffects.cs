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
        private LineRenderer burst;
        private readonly LineRenderer[] streaks = new LineRenderer[6];
        private Vector3 burstOrigin;
        private float burstTime;
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
            trail.time = .32f; trail.minVertexDistance = .05f;
            trail.startWidth = .85f; trail.endWidth = 0;
            trail.startColor = new Color(.25f, .9f, 1, .8f); trail.endColor = new Color(.8f, 1, 1, 0);
            trail.sortingOrder = 12; trail.emitting = false;
            burst = MakeLine("Dash shockwave", 32, true);
            for (int i = 0; i < streaks.Length; i++) streaks[i] = MakeLine("Dash speed streak " + i, 2, false);
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
        private LineRenderer MakeLine(string label, int count, bool loop)
        {
            var line = new GameObject(label).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.sharedMaterial = material; line.useWorldSpace = true;
            line.positionCount = count; line.loop = loop; line.sortingOrder = 18;
            line.numCapVertices = 4; line.enabled = false;
            return line;
        }
        private void Update()
        {
            if (Time.timeScale <= 0) return;
            trail.emitting = player.IsBoosting;
            burstTime = Mathf.Max(0, burstTime - Time.deltaTime);
            burst.enabled = burstTime > 0 && player.IsBoosting;
            if (burst.enabled)
            {
                float t = 1 - burstTime / .3f;
                burst.startWidth = burst.endWidth = .12f * (1 - t);
                burst.startColor = burst.endColor = new Color(.8f, 1, 1, 1 - t);
                for (int i = 0; i < 32; i++)
                {
                    float a = i * Mathf.PI * 2 / 32;
                    burst.SetPosition(i, burstOrigin + new Vector3(Mathf.Cos(a) * .4f, Mathf.Sin(a), 0) * (.4f + t * 1.2f));
                }
            }
            for (int i = 0; i < streaks.Length; i++)
            {
                var line = streaks[i]; line.enabled = player.IsBoosting;
                if (!line.enabled) continue;
                float phase = Mathf.Repeat(Time.time * 4 + i * .17f, 1);
                var tip = transform.position + new Vector3(.5f - phase * .8f, (i - 2.5f) * .28f, 0);
                line.startWidth = .025f; line.endWidth = .09f;
                line.startColor = new Color(.2f, .8f, 1, 0);
                line.endColor = new Color(.9f, 1, 1, .95f);
                line.SetPosition(0, tip + Vector3.left * (1.1f + phase));
                line.SetPosition(1, tip);
            }
        }
        private void Landed(ComboLanding result)
        {
            if (result != ComboLanding.Perfect) return;
            sound.pitch = 1 + Mathf.Min(player.Combo.Count - 1, 5) * .08f;
            sound.PlayOneShot(chime);
        }
        private void Boost() { sound.pitch = 1; sound.PlayOneShot(rush); burstOrigin = transform.position; burstTime = .3f; trail.emitting = true; }
        private void ResetEffects() { trail.Clear(); trail.emitting = false; sound.Stop(); burstTime = 0; burst.enabled = false; foreach (var line in streaks) line.enabled = false; }
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
