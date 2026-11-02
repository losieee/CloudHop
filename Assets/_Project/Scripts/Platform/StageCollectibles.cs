using System;
using System.Linq;
using UnityEngine;
namespace CloudHop
{
    [Serializable] public struct StageCoinLayout { public Vector2 position; public int value; }
    public sealed class StageCollectibles : MonoBehaviour
    {
        private CloudCoin[] coins, crystals;
        private Vector3[] crystalHomes;
        private AudioSource sound;
        private AudioClip goldSound, crystalSound;
        public int Collected { get; private set; }
        public int Total { get; private set; }
        public int SilverCount { get; private set; }
        public int GoldCount { get; private set; }
        public int Crystals { get; private set; }
        public bool CanFinish => Crystals > 0;
        public bool CompleteCollection => Crystals == 3;
        public int Medal => Total > 0 && Collected >= Total*.8f ? 3 : Total > 0 && Collected >= Total*.5f ? 2 : Collected > 0 ? 1 : 0;
        public string Notice { get; private set; }
        public float NoticeUntil { get; private set; }
        private void Awake() => EnsureCreated();
        private CloudCoin Spawn(string label, Vector3 position, int value, float phase)
        {
            var coin=new GameObject(label).AddComponent<CloudCoin>();
            coin.transform.SetParent(transform,false);coin.transform.position=position;
            coin.Configure(this,value,phase);return coin;
        }
        private void EnsureCreated()
        {
            if(coins!=null)return;
            var course=GetComponent<StageCourse>();
            var layouts=course.Definition.coins;
            if(layouts==null || layouts.Length==0) {
                var generated=new System.Collections.Generic.List<StageCoinLayout>();
                for(int i=1;i<course.Platforms.Length;i++) {
                    var from=course.Platforms[i-1].transform.position;
                    var to=course.Platforms[i].transform.position;
                    float duration=(to.x-from.x)/5f;
                    if(duration<=0)continue;
                    float vy=(to.y-from.y+.5f*19.62f*duration*duration)/duration;
                    for(int k=1;k<=3;k++){
                        float t=duration*k/4;
                        generated.Add(new StageCoinLayout{position=(Vector2)from+new Vector2(5*t,.8f+vy*t-.5f*19.62f*t*t),value=i%8==0&&k==2?5:1});
                    }
                }
                layouts=generated.ToArray();
            }
            coins=new CloudCoin[layouts.Length];
            for(int i=0;i<layouts.Length;i++) {
                int value=layouts[i].value>1?5:1;
                coins[i]=Spawn("Cloud Coin "+i,layouts[i].position,value,i*.7f);Total+=value;
            }
            crystals=new CloudCoin[3];crystalHomes=new Vector3[3];
            var branches=course.GetComponentsInChildren<BranchLanding>(true).OrderBy(x=>x.transform.position.x).ToArray();
            for(int i=0;i<3;i++) {
                var platform=i>0 && branches.Length>=i?branches[i-1].GetComponent<Platform>():course.Platforms[Mathf.Clamp(Mathf.RoundToInt(course.JumpCount*(.3f+i*.23f)),1,course.JumpCount-1)];
                var box=platform.GetComponent<Collider2D>();
                // Local geometry works even when a course is initially inactive.
                Vector3 pos=platform.transform.position+Vector3.up*.95f;
                if(box.enabled && box.bounds.size.y>0)pos=new Vector3(box.bounds.center.x,box.bounds.max.y+.65f,0);
                crystalHomes[i]=pos;crystals[i]=Spawn("Sky Crystal "+i,pos,0,i);
            }
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=0;sound.volume=.3f;
            goldSound=Tone(880);crystalSound=Tone(1320);
        }
        internal void Collect(int value) => Collected+=value;
        public void CollectItem(int value, PlayerController player)
        {
            if(value==0) { Crystals++; Notice=UIText.Get(CompleteCollection?"collect.complete":"collect.crystal",Crystals);sound.PlayOneShot(crystalSound); }
            else {
                Collected+=value;
                if(value>1) { GoldCount++;player.Combo.AddBoost();Notice=UIText.Get("collect.gold");sound.PlayOneShot(goldSound); }
                else SilverCount++;
            }
            if(value!=1)NoticeUntil=Time.time+1.5f;
        }
        public void RecallRequiredCrystal(Vector3 playerPosition)
        {
            EnsureCreated();
            if(CanFinish)return;
            crystals[0].Recall(playerPosition);
            Notice=UIText.Get("collect.recall");NoticeUntil=Time.time+2;
        }
        public void ResetCollection()
        {
            EnsureCreated();Collected=SilverCount=GoldCount=Crystals=0;Notice="";NoticeUntil=0;
            foreach(var coin in coins)coin.Restore();
            for(int i=0;i<crystals.Length;i++){crystals[i].Recall(crystalHomes[i]);crystals[i].Restore();}
        }
        private static AudioClip Tone(float frequency)
        {
            const int rate=22050;var data=new float[rate/3];
            for(int i=0;i<data.Length;i++){float t=(float)i/rate;data[i]=Mathf.Sin(2*Mathf.PI*frequency*t)*(1-(float)i/data.Length)*.3f;}
            var clip=AudioClip.Create("Collect reward",data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        private void OnDestroy(){Destroy(goldSound);Destroy(crystalSound);}
    }
}