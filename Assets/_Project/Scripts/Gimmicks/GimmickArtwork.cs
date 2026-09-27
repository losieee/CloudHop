using UnityEngine;

namespace CloudHop
{
    [DefaultExecutionOrder(300)]
    public sealed class GimmickArtwork : MonoBehaviour
    {
        [SerializeField] private Gimmick mechanic;
        [SerializeField] private SpriteRenderer marker;
        [SerializeField] private SpriteRenderer artwork;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private Vector2[] opaqueSizes;
        private StageMechanics clock;
        private float previousX;
        private void Awake()
        {
            if ((GetComponent<WindZone>() != null || GetComponent<LightningZone>() != null)
                && GetComponent<GimmickMotionEffects>() == null)
                gameObject.AddComponent<GimmickMotionEffects>();
        }
        private void OnEnable()
        {
            clock = GetComponentInParent<StageMechanics>();
            previousX = transform.position.x;
        }
        public void Configure(Gimmick source, SpriteRenderer original, SpriteRenderer art,
            Sprite[] sprites, Vector2[] sizes)
        {
            mechanic=source; marker=original; artwork=art; frames=sprites; opaqueSizes=sizes;
            Refresh();
        }
        private void LateUpdate() => Refresh();
        private void OnDisable()
        {
            if(marker!=null)marker.forceRenderingOff=false;
        }
        public void Refresh()
        {
            if(artwork==null || mechanic==null || frames==null || frames.Length!=10)return;
            if(clock==null)clock=GetComponentInParent<StageMechanics>();
            float time=clock==null ? 0 : clock.Elapsed;
            bool enabled=clock==null || clock.Enabled;
            int index=0;
            float alpha=enabled?1:.25f;
            Vector2 dimensions=Vector2.one;
            Vector3 position=Vector3.zero;
            float angle=0;
            bool beam=false;
            var collider=GetComponent<BoxCollider2D>();
            Vector2 worldSize=Vector2.Scale(collider.size,transform.lossyScale);
            if(mechanic is PatrolBird)
            {
                index=(Mathf.FloorToInt(time*6)%2);
                dimensions=new Vector2(1f,opaqueSizes[index].y/opaqueSizes[index].x);
                float delta=transform.position.x-previousX;
                if(Mathf.Abs(delta)>.0001f)artwork.flipX=delta<0;
                previousX=transform.position.x;
            }
            else if(mechanic is WindZone wind)
            {
                index=2; angle=90;
                dimensions=new Vector2(worldSize.y,worldSize.x);
                alpha=enabled && wind.IsBlowing ? .28f+.1f*Mathf.Sin(time*6) : .08f;
            }
            else if(mechanic is CrumblingPlatform cloud)
            {
                index=cloud.Phase==CloudPhase.Solid?3:cloud.Phase==CloudPhase.Crumbling?4:5;
                dimensions=new Vector2(worldSize.x,worldSize.x*opaqueSizes[index].y/opaqueSizes[index].x);
                position.y=collider.offset.y+collider.size.y*.5f;
                alpha=cloud.Phase==CloudPhase.Gone?.15f:1;
            }
            else if(mechanic is MovingPlatform)
            {
                index=6;
                dimensions=new Vector2(worldSize.x,worldSize.x*opaqueSizes[index].y/opaqueSizes[index].x);
                position.y=collider.offset.y+collider.size.y*.5f;
            }
            else if(mechanic is LightningZone bolt)
            {
                index=bolt.Phase==HazardPhase.Safe?9:bolt.Phase==HazardPhase.Warning?7:8;
                dimensions=new Vector2(1.6f,1.6f*opaqueSizes[index].y/opaqueSizes[index].x);
                position.y=collider.offset.y+collider.size.y*.5f;
                alpha=enabled ? bolt.Phase==HazardPhase.Safe?.35f:1 : .15f;
                beam=false; // The animated strike now shows the damage column.
            }
            artwork.sortingOrder = mechanic is CrumblingPlatform || mechanic is MovingPlatform ? 0 : 2;
            artwork.sprite=frames[index];
            artwork.color=new Color(1,1,1,alpha);
            artwork.transform.localPosition=position;
            artwork.transform.localRotation=Quaternion.Euler(0,0,angle);
            Vector3 scale=transform.lossyScale;
            artwork.transform.localScale=new Vector3(dimensions.x/opaqueSizes[index].x/Mathf.Abs(scale.x),
                dimensions.y/opaqueSizes[index].y/Mathf.Abs(scale.y),1);
            if(marker!=null)marker.forceRenderingOff=!beam;
        }
    }
}
