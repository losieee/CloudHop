using UnityEngine;

namespace CloudHop
{
    // Castle ward: the two gold seals warn before a straight magic curtain closes.
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MagicSealZone : PeriodicGimmick
    {
        private readonly LineRenderer[] rings = new LineRenderer[4];
        private readonly LineRenderer[] runes = new LineRenderer[7];
        private LineRenderer curtain;
        private Material ink;
        private BoxCollider2D area;
        public override string Status => $"SEAL {Phase.ToString().ToUpper()} {Remaining:F1}s";
        public override Color DebugColor => new Color(1f, .77f, .3f);

        public static MagicSealZone Replace(LightningZone bolt)
        {
            var obj = new GameObject(bolt.name.Replace("LightningZone", "MagicSealZone"));
            obj.transform.SetParent(bolt.transform.parent, false);
            obj.transform.localPosition = bolt.transform.localPosition;
            obj.transform.localRotation = bolt.transform.localRotation;
            obj.transform.localScale = bolt.transform.localScale;
            var source = bolt.GetComponent<BoxCollider2D>();
            var box = obj.AddComponent<BoxCollider2D>();
            box.size = source.size;
            box.offset = source.offset;
            box.isTrigger = true;
            var seal = obj.AddComponent<MagicSealZone>();
            seal.CopyTimingFrom(bolt);
            // Disable immediately; deferred destruction must never leave a second live hazard.
            bolt.gameObject.SetActive(false);
            Destroy(bolt.gameObject);
            return seal;
        }

        protected override void Awake()
        {
            base.Awake();
            area = GetComponent<BoxCollider2D>();
            ink = new Material(Shader.Find("Sprites/Default"));
            for (int i = 0; i < rings.Length; i++) rings[i] = Stroke("Gold Seal", 33, .045f);
            for (int i = 0; i < runes.Length; i++) runes[i] = Stroke("Floating Rune", 5, .035f);
            curtain = Stroke("Magic Curtain", 2, area.size.x);
            Refresh();
        }

        private LineRenderer Stroke(string label, int points, float width)
        {
            var line = new GameObject(label).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.useWorldSpace = false;
            line.sharedMaterial = ink;
            line.positionCount = points;
            line.widthMultiplier = width;
            line.sortingOrder = 3;
            return line;
        }

        protected override void LateUpdate() { base.LateUpdate(); Refresh(); }
        private void Refresh()
        {
            if (curtain == null) return;
            bool active = IsActive && Phase == HazardPhase.Active;
            bool warning = IsActive && Phase == HazardPhase.Warning;
            float alpha = !EnabledForCourse ? .12f : active ? 1f : warning ? .65f + .25f * Mathf.Sin(Elapsed * 16) : .3f;
            var gold = new Color(1f, .76f, .27f, alpha);
            float top = area.offset.y + area.size.y * .5f;
            float bottom = area.offset.y - area.size.y * .5f;
            for (int i = 0; i < rings.Length; i++)
            {
                float y = i < 2 ? top : bottom;
                float radius = i % 2 == 0 ? .58f : .4f;
                for (int p = 0; p < 33; p++)
                {
                    float angle = p * Mathf.PI / 16 + Elapsed * (i % 2 == 0 ? .4f : -.4f);
                    rings[i].SetPosition(p, new Vector3(area.offset.x + Mathf.Cos(angle) * radius, y + Mathf.Sin(angle) * radius * .3f, 0));
                }
                rings[i].startColor = rings[i].endColor = gold;
            }
            for (int i = 0; i < runes.Length; i++)
            {
                float y = Mathf.Lerp(bottom + .4f, top - .4f, i / 6f);
                float x = area.offset.x;
                float radius = active ? .16f : .1f;
                runes[i].SetPositions(new[] { new Vector3(x,y+radius), new Vector3(x+radius,y),
                    new Vector3(x,y-radius), new Vector3(x-radius,y), new Vector3(x,y+radius) });
                runes[i].startColor = runes[i].endColor = gold;
            }
            curtain.SetPosition(0, new Vector3(area.offset.x, bottom, 0));
            curtain.SetPosition(1, new Vector3(area.offset.x, top, 0));
            curtain.startColor = curtain.endColor = new Color(.8f, .45f, 1f, active ? .65f : warning ? .12f : 0);
        }

        private void OnTriggerEnter2D(Collider2D other) => Hit(other);
        private void OnTriggerStay2D(Collider2D other) => Hit(other);
        private void Hit(Collider2D other)
        {
            if (!IsActive || Phase != HazardPhase.Active) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null && !player.IsGrounded && player.TryKnockback(new Vector2(-4.5f, 3f)))
                CloudAudio.Cue("chop", .6f);
        }
        private void OnDestroy() { if (ink != null) Destroy(ink); }
    }
}
