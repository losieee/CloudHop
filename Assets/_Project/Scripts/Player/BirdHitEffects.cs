using UnityEngine;

namespace CloudHop
{
    // Reused per player. World-space sparks remain at the contact while the player recoils.
    public sealed class BirdHitEffects : MonoBehaviour
    {
        const float Duration = .42f;
        private readonly LineRenderer[] sparks = new LineRenderer[8];
        private readonly Vector3[] points = new Vector3[10];
        private LineRenderer ring;
        private Material material;
        private AudioSource sound;
        private AudioClip impact;
        private PlayerController player;
        private Vector3 contact;
        private float remaining;
        public bool IsPlaying => remaining > 0;
        public int BurstCount { get; private set; }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            material = new Material(Shader.Find("Sprites/Default"));
            ring = MakeLine("Bird impact ring", 32, true);
            for (int i = 0; i < sparks.Length; i++) sparks[i] = MakeLine("Bird impact spark " + i, i % 2 == 0 ? 10 : 2, i % 2 == 0);
            sound = gameObject.AddComponent<AudioSource>();
            sound.playOnAwake = false; sound.spatialBlend = 0; sound.volume = .22f;
            const int rate = 22050;
            var samples = new float[(int)(rate * .14f)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate, fade = 1f - (float)i / samples.Length;
                samples[i] = Mathf.Sin(2 * Mathf.PI * (340 * t - 750 * t * t)) * fade * fade * .65f;
            }
            impact = AudioClip.Create("Bird impact", samples.Length, 1, rate, false);
            impact.SetData(samples, 0);
        }
        private LineRenderer MakeLine(string label, int count, bool loop)
        {
            var line = new GameObject(label).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.sharedMaterial = material; line.useWorldSpace = true;
            line.positionCount = count; line.loop = loop; line.sortingOrder = 20;
            line.numCapVertices = 3; line.enabled = false;
            return line;
        }
        private void OnEnable() => player.ResetPerformed += Clear;
        private void OnDisable() { player.ResetPerformed -= Clear; Clear(); }
        public void Play(Vector3 position)
        {
            contact = position; contact.z = 0;
            remaining = Duration; BurstCount++;
            sound.PlayOneShot(impact);
            Draw(0);
        }
        private void Update()
        {
            if (!IsPlaying || Time.timeScale <= 0) return;
            remaining = Mathf.Max(0, remaining - Time.deltaTime);
            if (remaining == 0) { Clear(); return; }
            Draw(1 - remaining / Duration);
        }
        private void Draw(float t)
        {
            float fade = 1 - t;
            ring.enabled = true;
            ring.startWidth = ring.endWidth = .045f * fade;
            ring.startColor = ring.endColor = new Color(1, .98f, .8f, fade * .8f);
            for (int i = 0; i < 32; i++)
            {
                float angle = i * Mathf.PI * 2 / 32;
                ring.SetPosition(i, contact + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (.12f + t * .65f));
            }
            for (int i = 0; i < sparks.Length; i++)
            {
                var line = sparks[i]; line.enabled = true;
                float angle = i * Mathf.PI / 4 + .2f;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                var center = contact + direction * (.2f + t * .85f) + Vector3.down * t * t * .35f;
                line.startWidth = line.endWidth = .045f * fade;
                line.startColor = line.endColor = new Color(1, .75f + .2f * (i % 2), .15f, fade);
                if (i % 2 == 0)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        float a = k * Mathf.PI / 5 + t * 3;
                        points[k] = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * (k % 2 == 0 ? .14f : .065f) * (1 - t * .5f);
                    }
                    line.SetPositions(points);
                }
                else { line.SetPosition(0, center); line.SetPosition(1, center + direction * .2f * fade); }
            }
        }
        private void Clear()
        {
            remaining = 0;
            if (ring != null) ring.enabled = false;
            foreach (var spark in sparks) if (spark != null) spark.enabled = false;
            if (sound != null) sound.Stop();
        }
        private void OnDestroy() { Destroy(material); Destroy(impact); }
    }
}
