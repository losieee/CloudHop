using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CloudHop
{
    [Serializable]
    public sealed class TimeRecord
    {
        public string id;
        public string nickname;
        public long milliseconds;
        public string recordedUtc;
    }

    public sealed class LocalRankingStore
    {
        public const string DefaultKey="CloudHop.TimeRanking.v1";
        private readonly string key;
        [Serializable] private sealed class Saved { public int version=1; public List<TimeRecord> entries=new List<TimeRecord>(); }
        public LocalRankingStore(string key=DefaultKey){this.key=key;}
        public static bool TryNickname(string raw,out string nickname)
        {
            nickname=(raw??"").Normalize(NormalizationForm.FormC).Trim();
            if(nickname.Length<1 || nickname.Length>12)return false;
            foreach(char c in nickname)
                if(!(c>='a'&&c<='z') && !(c>='A'&&c<='Z') && !(c>='0'&&c<='9') &&
                   !(c>='\uAC00'&&c<='\uD7A3') && !(c>='\u1100'&&c<='\u11FF') &&
                   !(c>='\u3130'&&c<='\u318F') && c!=' ' && c!='_' && c!='-')return false;
            return true;
        }
        private Saved Parse(string json)
        {
            if(string.IsNullOrWhiteSpace(json))return null;
            try
            {
                var data=JsonUtility.FromJson<Saved>(json);
                if(data==null || data.version!=1 || data.entries==null)return null;
                data.entries=data.entries.Where(e=>e!=null && !string.IsNullOrEmpty(e.id) && e.milliseconds>0 && TryNickname(e.nickname,out _))
                    .GroupBy(e=>e.id).Select(g=>g.First()).OrderBy(e=>e.milliseconds).Take(10).ToList();
                return data;
            }
            catch(ArgumentException){return null;}
        }
        public List<TimeRecord> Load() => (Parse(PlayerPrefs.GetString(key,"")) ?? Parse(PlayerPrefs.GetString(key+".backup","")) ?? new Saved()).entries;
        // Each completed run is submitted once; equal times retain registration order.
        public int Register(TimedRun run)
        {
            if(run==null || !run.Completed)throw new InvalidOperationException("Only a full three-stage clear may be ranked.");
            var entries=Load();
            int found=entries.FindIndex(e=>e.id==run.Id);
            if(found>=0)return found+1;
            var record=new TimeRecord{id=run.Id,nickname=run.Nickname,milliseconds=(long)Math.Max(1,Math.Round(run.Seconds*1000)),recordedUtc=DateTime.UtcNow.ToString("o")};
            entries.Add(record);
            entries=entries.OrderBy(e=>e.milliseconds).ToList();
            int rank=entries.IndexOf(record)+1;
            var json=JsonUtility.ToJson(new Saved{entries=entries.Take(10).ToList()});
            var previous=PlayerPrefs.GetString(key,"");
            if(Parse(previous)!=null)PlayerPrefs.SetString(key+".backup",previous);
            PlayerPrefs.SetString(key,json);PlayerPrefs.Save();
            return rank<=10 ? rank : 0;
        }
    }
}
