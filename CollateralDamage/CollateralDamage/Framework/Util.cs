using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using Harmony;

namespace CollateralDamage.Framework
{
    public class Util
    {
        public class BuildingDestructionInfo
        {
            public int BuildingHealth;
            public int BuildingCost;
            public int Count;

            public BuildingDestructionInfo(int health, int cost, int count)
            {
                this.BuildingHealth = health;
                this.BuildingCost = cost;
                this.Count = count;
            }
        }

        public enum ObjectiveState
        {
            None = 0,
            Shown = 1,
            Succeeded = 2,
            SucceededFading = 3,
            Failed = 4,
            FailedFading = 5,
            Off = 6
        }
    }
}
