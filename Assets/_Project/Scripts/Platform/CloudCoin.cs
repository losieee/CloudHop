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
        private Transform collector;
        private LineRenderer halo; private Material haloMaterial;
        public bool IsCrystal => value == 0;
        public bool Collected { get; private set; }
        public int Value => value;
        public void Configure(StageCollectibles collection, int points, float offset)
        {
            owner = collection; value = points; phase = offset; origin = transform.localPosition;
            art = new GameObject("Coin artwork").AddComponent<SpriteRenderer>();
            art.transform.SetParent(transform, false); art.sprite = IsCrystal ? CloudCoinArt.Crystal : value > 1 ? CloudCoinArt.Sprite : CloudCoinArt.Silver; art.sortingOrder = 30;
            trigger = gameObject.AddComponent<CircleCollider2D>(); trigger.isTrigger = true; trigger.radius = IsCrystal ? .55f : value > 1 ? .43f : .32f;
            if(value!=1){
                halo=new GameObject("Reward sparkle").AddComponent<LineRenderer>();halo.transform.SetParent(transform,false);
                haloMaterial=new Material(Shader.Find("Sprites/Default"));halo.sharedMaterial=haloMaterial;
                halo.positionCount=16;halo.loop=true;halo.useWorldSpace=true;halo.sortingOrder=35;halo.enabled=false;
            }
            Restore();
        }
        public void Restore()
        {
            if(halo!=null)halo.enabled=false;
            Collected = false; age = 0; collector = null; transform.localPosition = origin; trigger.enabled = true; art.enabled = true;
            art.transform.localPosition = Vector3.zero; art.color = Color.white;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || Collected || Time.timeScale <= 0) return;
            Collected = true; trigger.enabled = false; age = 0;
            collector = player.transform;
            owner.CollectItem(value, player);
        }
        private void OnDestroy(){ if(haloMaterial!=null)Destroy(haloMaterial); }
        public void Recall(Vector3 position) { origin = transform.parent.InverseTransformPoint(position); transform.localPosition = origin; }
        private void Update()
        {
            float size = IsCrystal ? 2f : value > 1 ? 1.35f : 1;
            if (Collected)
            {
                age += Time.deltaTime;
                if(halo!=null){
                    halo.enabled=age<.5f;
                    float fade=Mathf.Clamp01(1-age/.5f);
                    halo.startWidth=halo.endWidth=.08f*fade;
                    halo.startColor=halo.endColor=IsCrystal?new Color(.6f,1,1,fade):new Color(1,.9f,.25f,fade);
                    for(int i=0;i<16;i++){float a=i*Mathf.PI/8;float radius=(.4f+age*2)*(i%2==0?1:.65f);halo.SetPosition(i,transform.position+new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*radius);}
                }
                art.transform.position = collector != null ? Vector3.Lerp(transform.position, collector.position, Mathf.Clamp01(age / .35f)) : transform.position;
                art.color = new Color(1, 1, 1, Mathf.Clamp01(1-age/.35f));
                art.transform.localScale = Vector3.one * size * (1 + age);
                if (age >= .35f) art.enabled = false;
            }
            else
            {
                transform.localPosition = origin + Vector3.up * Mathf.Sin(Time.time * 2 + phase) * .055f;
                art.transform.localScale = new Vector3(size * (.85f + .15f * Mathf.Sin(Time.time * 3 + phase)), size, 1);
                art.color = Color.white;
            }
        }
    }
}
