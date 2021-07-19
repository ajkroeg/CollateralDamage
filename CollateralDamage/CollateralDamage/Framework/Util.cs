using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
