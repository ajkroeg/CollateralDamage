using System.Collections.Generic;
using CollateralDamage.Framework;

namespace CollateralDamage
{
    public static class ModState
    {
        public static Dictionary<int, Util.BuildingDestructionInfo> BuildingsDestroyed = new Dictionary<int, Util.BuildingDestructionInfo>();
        public static int BuildingsDestroyedCount = 0;
        //public static int BuildingsDestroyedThreshold = 0;
        public static Dictionary<int, Util.BuildingDestructionInfo> BuildingsDestroyedByOpFor = new Dictionary<int, Util.BuildingDestructionInfo>();
        public static CollateralDamageInfo CurrentWhiteListInfo = new CollateralDamageInfo();
        public static bool HasObjective = false;
        public static int FinalPayResult;
        public static bool ShouldAutocomplete = true;
        public static bool HasSeenEvacPopup = false;
        public static bool StartFirstRoundOnce = false;

        public static void Reset()
        {
            BuildingsDestroyed = new Dictionary<int, Util.BuildingDestructionInfo>();
            BuildingsDestroyedCount = 0;
            //BuildingsDestroyedThreshold = 0;
            BuildingsDestroyedByOpFor = new Dictionary<int, Util.BuildingDestructionInfo>();
            CurrentWhiteListInfo = new CollateralDamageInfo();
            HasObjective = false;
            FinalPayResult = 0;
            ShouldAutocomplete = true;
            HasSeenEvacPopup = false;
            StartFirstRoundOnce = false;
        }
    }
}
