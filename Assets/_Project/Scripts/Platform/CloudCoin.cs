using UnityEngine;
namespace CloudHop
{
    public sealed class CloudCoin : MonoBehaviour
    {
        private StageCollectibles owner;
        private SpriteRenderer art;
        private CircleCollider2D trigger;
        private int value;
        private Vector3 origin;
        private float phase, age;
        public bool Collected { get; private set; }
        public int Value => value;
        public void Configure(StageCollectibles collection, int points, float offset)
        {
            owner = collection; value = points; phase = offset; origin = transform.localPosition;
            art = new GameObject("Coin artwork").AddComponent<SpriteRenderer>();
            art.transform.SetParent(transform, false); art.sprite = CloudCoinArt.Sprite; art.sortingOrder = 30;
            trigger = gameObject.AddComponent<CircleCollider2D>(); trigger.isTrigger = true; trigger.radius = value > 1 ? .43f : .32f;
            Restore();
        }
        public void Restore()
        {
            Collected = false; age = 0; trigger.enabled = true; art.enabled = true;
            art.transform.localPosition = Vector3.zero; art.color = Color.white;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || Collected || Time.timeScale <= 0) return;
            Collected = true; trigger.enabled = false; age = 0;
            owner.Collect(value);
        }
        private void Update()
        {
            float size = value > 1 ? 1.35f : 1;
            if (Collected)
            {
                age += Time.deltaTime;
                art.transform.localPosition = Vector3.up * age * 2;
                art.color = new Color(1, 1, 1, Mathf.Clamp01(1-age/.35f));
                art.transform.localScale = Vector3.one * size * (1 + age);
                if (age >= .35f) art.enabled = false;
            }
            else
            {
                transform.localPosition = origin + Vector3.up * Mathf.Sin(Time.time * 2 + phase) * .055f;
                art.transform.localScale = new Vector3(size * (.85f + .15f * Mathf.Sin(Time.time * 3 + phase)), size, 1);
                art.color = value > 1 ? new Color(1, .85f, .65f) : Color.white;
            }
        }
    }
}
