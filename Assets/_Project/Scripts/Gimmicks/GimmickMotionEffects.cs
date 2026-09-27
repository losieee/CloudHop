using UnityEngine;

namespace CloudHop
{
    // Visual-only geometry, created once and driven by the same clock as the hazard.
    [DefaultExecutionOrder(310)]
    public sealed class GimmickMotionEffects : MonoBehaviour
    {
        [SerializeField, Min(.1f)] private float windTravelSpeed = 1.1f;
        [SerializeField, Min(.01f)] private float windLineWidth = .06f;
        [SerializeField, Min(.01f)] private float boltCoreWidth = .045f;
        private WindZone wind;
        private LightningZone bolt;
        private StageMechanics clock;
        private BoxCollider2D area;
        private LineRenderer[] lines;
        private float strikeStarted;
        private bool wasStriking;
        private float previousTime;
        public int VisibleLineCount
        {
            get
            {
                int count=0;
                if(lines!=null)foreach(var line in lines)if(line.enabled)count++;
                return count;
            }
        }

        private void Awake()
        {
            wind=GetComponent<WindZone>();
            bolt=GetComponent<LightningZone>();
            clock=GetComponentInParent<StageMechanics>();
            area=GetComponent<BoxCollider2D>();
            var sprite=transform.Find("GimmickArt").GetComponent<SpriteRenderer>();
            lines=new LineRenderer[wind!=null?10:3];
            for(int i=0;i<lines.Length;i++)
            {
                var child=new GameObject(wind!=null?"Rising Wind "+i:"Lightning Stroke "+i);
                child.transform.SetParent(transform,false);
                var line=child.AddComponent<LineRenderer>();
                line.sharedMaterial=sprite.sharedMaterial;
                line.useWorldSpace=true;
                line.sortingOrder=4+i;
                line.numCapVertices=3;
                line.numCornerVertices=2;
                line.positionCount=wind!=null?8:13;
                line.enabled=false;
                lines[i]=line;
            }
        }

        private void OnDisable()
        {
            if(lines!=null)foreach(var line in lines)line.enabled=false;
            wasStriking=false;
        }

        private void LateUpdate()
        {
            float time=clock.Elapsed;
            if(time<previousTime)wasStriking=false;
            previousTime=time;
            bool visible=clock.Enabled && clock.Running &&
                (wind!=null?wind.IsBlowing:bolt.Phase==HazardPhase.Active);
            foreach(var line in lines)line.enabled=visible;
            if(!visible){wasStriking=false;return;}
            Bounds bounds=area.bounds;
            if(wind!=null)
            {
                for(int i=0;i<lines.Length;i++)
                {
                    float phase=Mathf.Repeat(time*windTravelSpeed+i*.137f,1);
                    float alpha=Mathf.Sin(phase*Mathf.PI)*.9f;
                    var line=lines[i];
                    line.widthMultiplier=windLineWidth;
                    line.startColor=new Color(.45f,.9f,1,0);
                    line.endColor=new Color(1,1,1,alpha);
                    for(int p=0;p<8;p++)
                    {
                        float t=p/7f;
                        float y=Mathf.Lerp(bounds.min.y,bounds.max.y,phase)-(.6f*(1-t));
                        y=Mathf.Max(bounds.min.y,y);
                        float x=bounds.center.x + Mathf.Sin(time*2+i*2.4f+t*.8f)*bounds.extents.x*.78f;
                        line.SetPosition(p,new Vector3(x,y,0));
                    }
                }
            }
            else
            {
                if(!wasStriking){strikeStarted=time;wasStriking=true;}
                float age=time-strikeStarted;
                float reach=Mathf.Clamp01(age/.035f);
                float flash=Mathf.Exp(-age*18);
                int shape=Mathf.FloorToInt(age*24);
                for(int i=0;i<lines.Length;i++)
                {
                    var line=lines[i];
                    float width=i==0?.30f:i==1?.13f:boltCoreWidth;
                    line.widthMultiplier=width*(1+flash*.7f);
                    Color color=i==0?new Color(1,.55f,.05f,.3f+flash*.3f)
                        :i==1?new Color(1,.9f,.2f,1):Color.white;
                    line.startColor=color;line.endColor=color;
                    for(int p=0;p<13;p++)
                    {
                        float t=p/12f;
                        float zig=p==0||p==12?0:Mathf.Sin(p*2.37f+shape*7.1f)*bounds.extents.x*.78f;
                        line.SetPosition(p,new Vector3(bounds.center.x+zig,
                            Mathf.Lerp(bounds.max.y,bounds.min.y,t*reach),0));
                    }
                }
            }
        }
    }
}
