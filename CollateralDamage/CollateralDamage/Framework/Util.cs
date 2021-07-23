using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using BattleTech.Designed;
using BattleTech.Framework;
using Harmony;
using HBS.Util;
using UnityEngine;

namespace CollateralDamage.Framework
{
    public class Util
    {
        public static GameObject NewGameObject(GameObject parent, string name = null)
        {
            return new GameObject(name ?? "Objective")
            {
                transform =
                {
                    parent = parent.transform,
                    localPosition = Vector3.zero
                }
            };
        }
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

        public class AvoidCollateralObjective : ObjectiveGameLogic
        {
            public override string GenerateJSONTemplate()
            {
                return JSONSerializationUtility.ToJSON<AvoidCollateralObjective>(new AvoidCollateralObjective());
            }
            public override string ToJSON()
            {
                return JSONSerializationUtility.ToJSON<AvoidCollateralObjective>(this);
            }
            public override void FromJSON(string json)
            {
                JSONSerializationUtility.FromJSON<AvoidCollateralObjective>(this, json);
            }

            public new CombatGameState Combat { get; set; }

            public override bool CheckForSuccess()
            {
                base.CheckForSuccess();
                return false;
            }
            public override bool CheckForFailure()
            {
                base.CheckForFailure();
                if (ModState.BuildingsDestroyed.Count > 0)
                {
                    ModInit.modLog.LogMessage(
                        $"Collateral Damage Detected! Objective Failed!");
                    return true;
                }
                return false;
            }
        }
    }
}
