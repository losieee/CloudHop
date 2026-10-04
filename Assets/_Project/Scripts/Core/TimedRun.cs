using System;

namespace CloudHop
{
    // A monotonic clock deliberately includes pause, retries and stage-result screens.
    public sealed class TimedRun
    {
        private readonly Func<double> clock;
        private double startedAt;
        private double finishedSeconds;
        public string Id { get; private set; }
        public string Nickname { get; private set; }
        public int ClearedStages { get; private set; }
        public bool Active { get; private set; }
        public bool Completed { get; private set; }
        public double Seconds => Completed ? finishedSeconds : Active ? Math.Max(0,clock()-startedAt) : 0;

        public TimedRun(Func<double> clock) { this.clock=clock; }
        public void Begin(string nickname)
        {
            if(!LocalRankingStore.TryNickname(nickname,out var valid))throw new ArgumentException("Invalid nickname");
            Nickname=valid;Id=Guid.NewGuid().ToString("N");ClearedStages=0;
            startedAt=clock();finishedSeconds=0;Completed=false;Active=true;
        }
        public bool ClearStage(int index)
        {
            if(!Active || index!=ClearedStages)return false;
            ClearedStages++;
            if(ClearedStages!=3)return false;
            finishedSeconds=Math.Max(.001,clock()-startedAt);Active=false;Completed=true;
            return true;
        }
        public void Cancel(){Active=false;Completed=false;ClearedStages=0;}
        public static string Format(double seconds)
        {
            long ms=(long)Math.Max(0,Math.Round(seconds*1000));
            return $"{ms/60000:00}:{ms/1000%60:00}.{ms%1000:000}";
        }
    }
}
