using NUnit.Framework;
using UnityEngine;

namespace CloudHop.Tests
{
    public sealed class RankingTests
    {
        private const string Key="CloudHop.TestOnly.Ranking";
        [TearDown] public void Cleanup(){PlayerPrefs.DeleteKey(Key);PlayerPrefs.DeleteKey(Key+".backup");PlayerPrefs.Save();}
        [Test] public void ClockIncludesWaitingAndRequiresAllStagesInOrder()
        {
            double now=10;var run=new TimedRun(()=>now);run.Begin("구름1");
            Assert.IsFalse(run.ClearStage(2));Assert.AreEqual(0,run.ClearedStages);
            now=20;run.ClearStage(0);Assert.IsFalse(run.ClearStage(0));
            now=50;Assert.AreEqual(40,run.Seconds);run.ClearStage(1);
            now=70;Assert.IsTrue(run.ClearStage(2));now=100;
            Assert.AreEqual(60,run.Seconds);Assert.IsFalse(run.ClearStage(2));
            run.Cancel();Assert.IsFalse(run.Completed);
        }
        [Test] public void TopTenPersistFastestFirstAndSubmissionIsIdempotent()
        {
            var store=new LocalRankingStore(Key);double now=0;
            for(int i=12;i>=1;i--)
            {
                var run=new TimedRun(()=>now);run.Begin("PLAYER"+i);
                now+=i;run.ClearStage(0);run.ClearStage(1);run.ClearStage(2);
                store.Register(run);store.Register(run);
            }
            var loaded=new LocalRankingStore(Key).Load();
            Assert.AreEqual(10,loaded.Count);Assert.AreEqual(1000,loaded[0].milliseconds);Assert.AreEqual(10000,loaded[9].milliseconds);
            Assert.AreEqual("PLAYER1",loaded[0].nickname);
            PlayerPrefs.SetString(Key,"{broken json");Assert.Greater(store.Load().Count,0);
        }
        [Test] public void InvalidNamesAndIncompleteRunsCannotRegister()
        {
            foreach(var name in new[]{"", "   ","<color=red>X", "abcdefghijklmn", "a\nb"})Assert.IsFalse(LocalRankingStore.TryNickname(name,out _));
            Assert.IsTrue(LocalRankingStore.TryNickname("  구름여우1  ",out var valid));Assert.AreEqual("구름여우1",valid);
            var run=new TimedRun(()=>0);run.Begin("AAA");
            Assert.Throws<System.InvalidOperationException>(()=>new LocalRankingStore(Key).Register(run));
        }
    }
}
