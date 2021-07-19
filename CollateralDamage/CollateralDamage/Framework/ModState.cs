using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollateralDamage.Framework;

namespace CollateralDamage
{
    public static class ModState
    {
        public static Dictionary<int, Util.BuildingDestructionInfo> BuildingsDestroyed = new Dictionary<int, Util.BuildingDestructionInfo>();
        public static Dictionary<int, Util.BuildingDestructionInfo> BuildingsDestroyedByOpFor = new Dictionary<int, Util.BuildingDestructionInfo>();
        public static bool HasObjective = false;
        public static bool HasBonusObjective = false;

        public static void Reset()
        {
            BuildingsDestroyed = new Dictionary<int, Util.BuildingDestructionInfo>();
            BuildingsDestroyedByOpFor = new Dictionary<int, Util.BuildingDestructionInfo>();
            HasObjective = false;
            HasBonusObjective = false;
        }
    }
}
