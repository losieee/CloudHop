using UnityEngine;

namespace CloudHop
{
    // Reused per player. World-space sparks remain at the contact while the player recoils.
    public sealed class BirdHitEffects : MonoBehaviour
    {
        const float Duration = .68f;
        private readonly LineRenderer[] sparks = new LineRenderer[16];
        private readonly Vector3[] points = new Vector3[10];
        private LineRenderer ring, echo, flash;
        private readonly LineRenderer[] debris = new LineRenderer[6];
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
            echo = MakeLine("Bird impact echo", 32, true);
            flash = MakeLine("Bird impact flash", 16, true);
            for (int i = 0; i < debris.Length; i++) debris[i] = MakeLine("Bird impact debris " + i, 3, false);
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
            line.positionCount = count; line.loop = loop; line.sortingOrder = 40;
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
            DrawRing(ring, .2f + Mathf.Sqrt(t) * 1.7f, .11f * fade, new Color(1, .97f, .78f, fade));
            float echoTime = Mathf.Clamp01((t - .12f) / .88f);
            echo.enabled = t >= .12f;
            if (echo.enabled) DrawRing(echo, .15f + echoTime * 1.65f, .065f * fade, new Color(.65f, .9f, 1, fade * .8f));
            float flashFade = Mathf.Clamp01(1 - t / .32f);
            flash.enabled = flashFade > 0;
            flash.startWidth = flash.endWidth = .16f * flashFade;
            flash.startColor = flash.endColor = new Color(1, 1, .88f, flashFade);
            for (int i = 0; i < 16; i++)
            {
                float a = i * Mathf.PI / 8;
                float radius = (i % 2 == 0 ? .8f : .2f) * (.7f + t * 2);
                flash.SetPosition(i, contact + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * radius);
            }
            for (int i = 0; i < debris.Length; i++)
            {
                var fragment = debris[i];
                fragment.enabled = true;
                float a = i * Mathf.PI * 2 / debris.Length + .4f;
                var direction = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0);
                var center = contact + direction * (.3f + t * 2.1f) + Vector3.down * t * t * 1.7f;
                var axis = new Vector3(Mathf.Cos(a + t * 9), Mathf.Sin(a + t * 9), 0) * .18f * fade;
                fragment.startWidth = .12f * fade; fragment.endWidth = .025f * fade;
                fragment.startColor = fragment.endColor = new Color(.82f, .95f, 1, fade);
                fragment.SetPosition(0, center - axis);
                fragment.SetPosition(1, center + Vector3.up * .07f * fade);
                fragment.SetPosition(2, center + axis);
            }
            for (int i = 0; i < sparks.Length; i++)
            {
                var line = sparks[i]; line.enabled = true;
                float angle = i * Mathf.PI * 2 / sparks.Length + .2f;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                var center = contact + direction * (.35f + Mathf.Sqrt(t) * 1.85f) + Vector3.down * t * t * .35f;
                line.startWidth = line.endWidth = .085f * fade;
                line.startColor = line.endColor = new Color(1, .75f + .2f * (i % 2), .15f, fade);
                if (i % 2 == 0)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        float a = k * Mathf.PI / 5 + t * 3;
                        points[k] = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * (k % 2 == 0 ? .27f : .12f) * (1 - t * .5f);
                    }
                    line.SetPositions(points);
                }
                else { line.SetPosition(0, center); line.SetPosition(1, center + direction * .45f * fade); }
            }
        }
        private void DrawRing(LineRenderer line, float radius, float width, Color color)
        {
            line.enabled = true;
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            for (int i = 0; i < 32; i++)
            {
                float angle = i * Mathf.PI * 2 / 32;
                line.SetPosition(i, contact + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius);
            }
        }
        private void Clear()
        {
            remaining = 0;
            if (ring != null) ring.enabled = false;
            if (echo != null) echo.enabled = false;
            if (flash != null) flash.enabled = false;
            foreach (var fragment in debris) if (fragment != null) fragment.enabled = false;
            foreach (var spark in sparks) if (spark != null) spark.enabled = false;
            if (sound != null) sound.Stop();
        }
        private void OnDestroy() { Destroy(material); Destroy(impact); }
    }
}
