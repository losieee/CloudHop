using System.Collections.Generic;

namespace CloudHop
{
    public enum ComboLanding { None, Perfect, Broken }

    // One charge maximum. Revisiting a platform can never farm a charge.
    public sealed class ComboChain
    {
        public const int Required = 3;
        public int Count { get; private set; }
        public bool BoostReady { get; private set; }
        private readonly HashSet<int> visited = new HashSet<int>();
        public void Remember(int platform) => visited.Add(platform);
        public ComboLanding Land(int platform, bool perfect)
        {
            if (!visited.Add(platform) || !perfect) return Break();
            Count++;
            if (Count % Required == 0) BoostReady = true;
            return ComboLanding.Perfect;
        }
        public ComboLanding Break()
        {
            bool hadCombo = Count > 0;
            Count = 0;
            return hadCombo ? ComboLanding.Broken : ComboLanding.None;
        }
        public bool ConsumeBoost()
        {
            if (!BoostReady) return false;
            BoostReady = false;
            return true;
        }
        public void Reset() { Count = 0; BoostReady = false; visited.Clear(); }
    }
}
