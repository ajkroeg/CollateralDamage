using BattleTech;
using BattleTech.Framework;
using HBS.Util;
using UnityEngine;

namespace CollateralDamage.Framework
{
    public class CollateralDamageInfo
    {
        public string ContractID = "";
        public bool DoWarCrimes = false;
        public int DestructionThreshold = 0;
        public int DestructionCap = 0;
        public float CBillResultOverride = 0;
        public float EmployerRepResult = 0;
        public float TargetRepResult = 0;
    }
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
        //public static MethodInfo _SetProgressText = AccessTools.Method(typeof(CombatHUDObjectiveItem), "SetProgressText");
        public static void LogSettings()
        {
            ModInit.modLog.LogMessage($"EmployerPlanetsOnly: {ModInit.modSettings.EmployerPlanetsOnly}");
            ModInit.modLog.LogMessage($"SupportOrAllyCosts: {ModInit.modSettings.SupportOrAllyCosts}");
            ModInit.modLog.LogMessage($"SizeFactor: {ModInit.modSettings.SizeFactor}");
            ModInit.modLog.LogMessage($"FlatRate: {ModInit.modSettings.FlatRate}");
            ModInit.modLog.LogMessage($"ContractPayFactorDmg: {ModInit.modSettings.ContractPayFactorDmg}");
            ModInit.modLog.LogMessage($"PublicNuisanceDamageOffset: {ModInit.modSettings.PublicNuisanceDamageOffset}");
            ModInit.modLog.LogMessage(
                $"CollateralDamageObjectiveChance: {ModInit.modSettings.CollateralDamageObjectiveChance}");
            ModInit.modLog.LogMessage($"ContractPayFactorBonus: {ModInit.modSettings.ContractPayFactorBonus}");
            ModInit.modLog.LogMessage($"CDThresholdMin: {ModInit.modSettings.CDThresholdMin}");
            ModInit.modLog.LogMessage($"CDThresholdMax: {ModInit.modSettings.CDThresholdMax}");

            foreach (var WLC in ModInit.modSettings.WhitelistedContracts)
            {
                ModInit.modLog.LogMessage($"//////WHITELISTED//////////");
                ModInit.modLog.LogMessage($"Whitelisted: ContractID: {WLC.ContractID}");
                ModInit.modLog.LogMessage($"Whitelisted: DoWarCrimes: {WLC.DoWarCrimes}");
                ModInit.modLog.LogMessage($"Whitelisted: DestructionThreshold: {WLC.DestructionThreshold}");
                ModInit.modLog.LogMessage($"Whitelisted: CBillResultOverride: {WLC.CBillResultOverride}");
                ModInit.modLog.LogMessage($"Whitelisted: EmployerRepResult: {WLC.EmployerRepResult}");
                ModInit.modLog.LogMessage($"Whitelisted: TargetRepResult: {WLC.TargetRepResult}");
                ModInit.modLog.LogMessage($"//////////////////////\n");
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
