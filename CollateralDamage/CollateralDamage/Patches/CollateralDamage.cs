using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using BattleTech.Framework;
using BattleTech.UI;
using CollateralDamage.Framework;
using Harmony;
using Localize;
using UnityEngine;

namespace CollateralDamage.Patches
{
    class CollateralDamage
    {
        [HarmonyPatch(typeof(TurnDirector), "StartFirstRound")]
        public static class TurnDirector_StartFirstRound
        {
            public static void Postfix(TurnDirector __instance)
            {
                var sim = UnityGameInstance.BattleTechGame.Simulation;
                if (sim == null) return;
                var contract = __instance.Combat.ActiveContract;

                var roll = ModInit.Random.NextDouble();
                if (roll <= ModInit.modSettings.CollateralDamageObjectiveChance && (ModInit.modSettings.EmployerPlanetsOnly &&
                    contract.Override.employerTeam.FactionValue.Name ==
                    sim.CurSystem.OwnerValue.Name || !ModInit.modSettings.EmployerPlanetsOnly) || ModInit.modSettings.WhitelistedContractIDs.Contains(contract.Override.ID))
                {
                    ModInit.modLog.LogMessage(
                        $"Contract whitelisted or roll {roll} <= threshold {ModInit.modSettings.CollateralDamageObjectiveChance}, creating secondary objective to avoid collateral damage.");

                    ModState.HasObjective = true;

                    if (__instance.Combat.MapMetaData.biomeSkin == Biome.BIOMESKIN.urbanHighTech || ModInit.modSettings.WhitelistedContractIDs.Contains(contract.Override.ID))
                    {
                        ModInit.modLog.LogMessage(
                            $"Map is urban or contract is whitelisted, bonus objective will pay.");
                        ModState.HasBonusObjective = true;
                    }

                    var HUD = Traverse.Create(CameraControl.Instance).Property("HUD").GetValue<CombatHUD>();
                    var objectivesList = HUD.ObjectivesList;
                    var objectiveItem = UnityEngine.Object.Instantiate<CombatHUDObjectiveItem>(objectivesList.secondaryObjectivePrefab);
                    objectiveItem.transform.SetParent(objectivesList.objectivesStack.transform);
                    objectiveItem.transform.localScale = Vector3.one;
                    var objectiveUIList = Traverse.Create(objectivesList).Field("objectiveUIItems").GetValue<List<CombatHUDObjectiveItem>>();
                    objectiveUIList.Add(objectiveItem);

                    objectiveItem.Init(new Text("Avoid Collateral Damage"), false, false);
                    objectiveItem.SetStatusColors(objectivesList.objectiveSucceeded.color, objectivesList.objectiveFailed.color);
                    objectiveItem.gameObject.SetActive(true);

                    //ADD objective to UI somehow?

                }
                else
                {
                    ModInit.modLog.LogMessage(
                        $"Contract not whitelisted or roll {roll} > threshold {ModInit.modSettings.CollateralDamageObjectiveChance}. No secondary objective will be created, no collateral damage will be penalized.");
                }
            }
        }


        [HarmonyPatch(typeof(BattleTech.Building), "FlagForDeath")]
        public static class Building_FlagForDeath
        {
            public static void Prefix(BattleTech.Building __instance, string reason, DeathMethod deathMethod,
                DamageType damageType, int location, int stackItemID, string attackerID, bool isSilent,
                bool ____flaggedForDeath, bool ___isObjectiveTarget)
            {
                if (__instance.Combat.ActiveContract.ContractTypeValue.IsSkirmish) return;

                if (____flaggedForDeath) return;
                if (__instance.team.GUID != "421027ec-8480-4cc6-bf01-369f84a22012" || ___isObjectiveTarget)
                    return; // is not Team "World" or is objective target
                if (ModInit.modSettings.SizeFactor == 0f && ModInit.modSettings.ContractPayFactorDmg == 0f &&
                    ModInit.modSettings.FlatRate == 0)
                {
                    ModInit.modLog.LogMessage(
                        $"All of SizeFactor, ContractPayFactor, and FlatRate == 0. Check config.");
                    return;
                }

                var attackingUnit = __instance.Combat.FindActorByGUID(attackerID);

                if (ModInit.modSettings.PublicNuisanceDamageOffset > 0f)
                {
                    if (__instance.team.GUID == "be77cadd-e245-4240-a93e-b99cc98902a5" || // Target
                        __instance.team.GUID == "31151ed6-cfc2-467e-98c4-9ae5bea784cf" || // TargetsAlly
                        __instance.team.GUID == "3c9f3a20-ab03-4bcb-8ab6-b1ef0442bbf0") // HostileToAll
                    {
                        var size = -1;

                        var flatRate = ModInit.modSettings.FlatRate;
                        var contractValue = __instance.Combat.ActiveContract.InitialContractValue *
                                            ModInit.modSettings.ContractPayFactorDmg;
                        var totalCost = flatRate + contractValue;
                        ModInit.modLog.LogMessage(
                            $"Offset Cost before size is {totalCost} from flatRate {flatRate} + contractValue {contractValue}.");

                        if (ModInit.modSettings.SizeFactor > 0f)
                        {
                            size = Mathf.RoundToInt(__instance.StartingStructure + __instance.StartingArmor);
                            var sizeCost = size * ModInit.modSettings.SizeFactor;
                            ModInit.modLog.LogMessage(
                                $"Offset Size cost is {sizeCost} from size {size} * SizeFactor {ModInit.modSettings.SizeFactor}.");
                            totalCost += sizeCost;
                        }

                        var roundedCost = Mathf.RoundToInt(totalCost);
                        ModInit.modLog.LogMessage(
                            $"Offset Final cost is {roundedCost}, generating cost object for AAR.");
                        if (!ModState.BuildingsDestroyedByOpFor.ContainsKey(size))
                        {
                            var buildingDestroyed = new Util.BuildingDestructionInfo(size, roundedCost, 1);
                            ModState.BuildingsDestroyedByOpFor.Add(size, buildingDestroyed);
                        }
                        else
                        {
                            ModState.BuildingsDestroyedByOpFor[size].Count += 1;
                        }
                    }
                }

                if (ModInit.modSettings.SupportOrAllyCosts &&
                    attackingUnit.team.GUID != "9ed02e70-beff-4131-952e-49d366e2f7cc" && // PlayerOneSupport
                    attackingUnit.team.GUID != "70af7e7f-39a8-4e81-87c2-bd01dcb01b5e" && // EmployersAlly
                    !attackingUnit.team.IsLocalPlayer) return;
                if (!ModInit.modSettings.SupportOrAllyCosts && attackingUnit.team.IsLocalPlayer) return;

                if (__instance.team.GUID != "be77cadd-e245-4240-a93e-b99cc98902a5" && // Target
                    __instance.team.GUID != "31151ed6-cfc2-467e-98c4-9ae5bea784cf" && // TargetsAlly
                    __instance.team.GUID != "3c9f3a20-ab03-4bcb-8ab6-b1ef0442bbf0") // HostileToAll
                {
                    var size = -1;

                    var flatRate = ModInit.modSettings.FlatRate;
                    var contractValue = __instance.Combat.ActiveContract.InitialContractValue *
                                        ModInit.modSettings.ContractPayFactorDmg;
                    var totalCost = flatRate + contractValue;
                    ModInit.modLog.LogMessage(
                        $"Cost before size is {totalCost} from flatRate {flatRate} + contractValue {contractValue}.");

                    if (ModInit.modSettings.SizeFactor > 0f)
                    {
                        size = Mathf.RoundToInt(__instance.StartingStructure + __instance.StartingArmor);
                        var sizeCost = size * ModInit.modSettings.SizeFactor;
                        ModInit.modLog.LogMessage(
                            $"Size cost is {sizeCost} from size {size} * SizeFactor {ModInit.modSettings.SizeFactor}.");
                        totalCost += sizeCost;
                    }

                    var roundedCost = Mathf.RoundToInt(totalCost);
                    ModInit.modLog.LogMessage(
                        $"Final cost is {roundedCost}, generating cost object for AAR.");
                    if (!ModState.BuildingsDestroyed.ContainsKey(size))
                    {
                        var buildingDestroyed = new Util.BuildingDestructionInfo(size, roundedCost, 1);
                        ModState.BuildingsDestroyed.Add(size, buildingDestroyed);
                    }
                    else
                    {
                        ModState.BuildingsDestroyed[size].Count += 1;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AAR_ContractObjectivesWidget), "FillInObjectives")]
        public static class AAR_ContractObjectivesWidget_FillInObjectives_Patch
        {
            public static void Postfix(AAR_ContractObjectivesWidget __instance, Contract ___theContract)
            {
                if (UnityGameInstance.BattleTechGame.Simulation == null) return;

                var addObjectiveMethod = Traverse.Create(__instance)
                    .Method("AddObjective", new Type[] {typeof(MissionObjectiveResult)});


                if (ModState.HasObjective && ModState.BuildingsDestroyed.Count == 0 && (ModInit.modSettings.WhitelistedContractIDs.Contains(___theContract.Override.ID) || ModState.HasBonusObjective))
                {
                    var bonus = 0;
                    bonus += Mathf.RoundToInt(___theContract.InitialContractValue *
                                              ModInit.modSettings.ContractPayFactorBonus);
                    bonus += ModInit.modSettings.FlatRateBonus;
                    ModInit.modLog.LogMessage($"{bonus} in collateral damage bonuses!");
                    var bldDestructResult = new MissionObjectiveResult($"Avoid Collateral Damage: ¢{bonus} bonus.", Guid.NewGuid().ToString(),
                        false, true, ObjectiveStatus.Succeeded, false);
                    addObjectiveMethod.GetValue(bldDestructResult);

                    var moneyResultsBonus = ___theContract.MoneyResults + bonus;
                    Traverse.Create(___theContract).Property("MoneyResults").SetValue(moneyResultsBonus);
                    ModState.Reset();
                    return;
                }

                var finalDamageCost = 0;
                if (ModState.HasObjective && ModState.BuildingsDestroyed.Count == 1 || ModState.BuildingsDestroyed.FirstOrDefault().Key == 1)
                {
                    var destructCostInfo = ModState.BuildingsDestroyed.FirstOrDefault();
                    var totalCost = destructCostInfo.Value.BuildingCost * destructCostInfo.Value.Count;
                    var bldDestructCost =
                        $"Collateral Damage Fee:  {destructCostInfo.Value.Count} Buildings x {destructCostInfo.Value.BuildingCost} ea. = ¢-{totalCost}";
                    var bldDestructResult = new MissionObjectiveResult($"{bldDestructCost}", Guid.NewGuid().ToString(),
                        false, true, ObjectiveStatus.Failed, false);
                    addObjectiveMethod.GetValue(bldDestructResult);
                    finalDamageCost = totalCost;
                }

                else if (ModState.HasObjective && ModState.BuildingsDestroyed.Count > 1)
                {
                    foreach (var bldgDestroyed in ModState.BuildingsDestroyed)
                    {
                        var totalCost = bldgDestroyed.Value.BuildingCost * bldgDestroyed.Value.Count;
                        var bldDestructCost =
                            $"Collateral Damage Fees:  {bldgDestroyed.Value.Count} Size {bldgDestroyed.Key} Buildings x {bldgDestroyed.Value.BuildingCost} ea. = ¢-{totalCost}";
                        var bldDestructResult = new MissionObjectiveResult($"{bldDestructCost}", Guid.NewGuid().ToString(),
                            false, true, ObjectiveStatus.Failed, false);
                        addObjectiveMethod.GetValue(bldDestructResult);
                        finalDamageCost += totalCost;
                        ModInit.modLog.LogMessage($"{totalCost} in collateral damage fees for size {bldgDestroyed.Key}. Current Total Fees: {finalDamageCost}");
                    }
                    
                }

                var finalDamageMitigation = 0;
                if (ModState.HasObjective && ModInit.modSettings.PublicNuisanceDamageOffset > 0f && ModState.BuildingsDestroyedByOpFor.Count > 0)
                {
                    foreach (var bldgDestroyedOp in ModState.BuildingsDestroyedByOpFor)
                    {
                        var totalOffset = Mathf.RoundToInt(bldgDestroyedOp.Value.BuildingCost * bldgDestroyedOp.Value.Count * ModInit.modSettings.PublicNuisanceDamageOffset);
                        var bldDestructOpCost =
                            $"Maximum Damage Mitigation:  {bldgDestroyedOp.Value.Count} Size {bldgDestroyedOp.Key} Buildings x {bldgDestroyedOp.Value.BuildingCost} ea. = ¢{totalOffset}";
                        var bldDestructOpResult = new MissionObjectiveResult($"{bldDestructOpCost}", Guid.NewGuid().ToString(),
                            false, true, ObjectiveStatus.Succeeded, false);
                        addObjectiveMethod.GetValue(bldDestructOpResult);
                        finalDamageMitigation += totalOffset;
                        ModInit.modLog.LogMessage($"{totalOffset} in collateral damage mitigation for size {bldgDestroyedOp.Key}. Current Total Offset: {finalDamageMitigation}");
                    }
                }

                var endingCosts = Math.Min(0, finalDamageCost - finalDamageMitigation);
                var moneyResults = ___theContract.MoneyResults - endingCosts;
                Traverse.Create(___theContract).Property("MoneyResults").SetValue(moneyResults);
                ModState.Reset();
            }
        }
    }
}
