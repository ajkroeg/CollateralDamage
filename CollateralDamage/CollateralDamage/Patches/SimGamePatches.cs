using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Framework;
using BattleTech.UI;
using UnityEngine;

namespace CollateralDamage.Patches
{
    public class SimGamePatches
    {
        [HarmonyPatch(typeof(AAR_ContractObjectivesWidget), "FillInObjectives")]
        public static class AAR_ContractObjectivesWidget_FillInObjectives_Patch
        {
            public static void Postfix(AAR_ContractObjectivesWidget __instance)
            {
                ModInit.modLog.LogMessage($"FillInObjectives: Dumping ModState");
                ModInit.modLog.LogMessage($"FillInObjectives: HasObjective:{ModState.HasObjective}");
                ModInit.modLog.LogMessage(
                    $"FillInObjectives: BuildingsDestroyed.Count:{ModState.BuildingsDestroyed.Count}");
                ModInit.modLog.LogMessage(
                    $"FillInObjectives: BuildingsDestroyedCount:{ModState.BuildingsDestroyedCount}");
                ModInit.modLog.LogMessage(
                    $"FillInObjectives: DoesWarCrimes:{ModState.CurrentWhiteListInfo.DoWarCrimes}");

                if (UnityGameInstance.BattleTechGame.Simulation == null) return;

                //var addObjectiveMethod = Traverse.Create(__instance).Method("AddObjective", new Type[] {typeof(MissionObjectiveResult)});
                var bonus = 0f;
                var penalty = 0f;
                var mitigation = 0f;
                if (ModState.HasObjective && ModState.BuildingsDestroyed.Count == 0)
                {
                    if (!ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        bonus += __instance.theContract.InitialContractValue *
                                 ModInit.modSettings.ContractPayFactorBonus;
                        bonus += ModInit.modSettings.FlatRateBonus;

                        //var bonusINT = Mathf.RoundToInt(bonus);

                        bonus *= ModState.CurrentWhiteListInfo.DestructionThreshold;
                        ModInit.modLog.LogMessage($"0D_NWC {bonus} in collateral damage bonuses!");
                        var bldDestructResult = new MissionObjectiveResult(
                            $"SUCCESS: Avoid Collateral Damage. Bonus to be awarded.",
                            Guid.NewGuid().ToString(),
                            false, true, ObjectiveStatus.Succeeded, false);
                        __instance.AddObjective(bldDestructResult);
                        //addObjectiveMethod.GetValue(bldDestructResult);
                    }

                    if (ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        penalty += __instance.theContract.InitialContractValue *
                                   ModInit.modSettings.ContractPayFactorBonus;
                        penalty += ModInit.modSettings.FlatRateBonus;

                        //var penaltyINT = Mathf.RoundToInt(penalty);
                        penalty *= ModState.CurrentWhiteListInfo.DestructionThreshold;
                        ModInit.modLog.LogMessage($"0D_NWC {penalty} in failed collateral damage fees!");
                        var bldDestructResult = new MissionObjectiveResult(
                            $"FAILED: Inflict Collateral Damage. Fees to be assessed",
                            Guid.NewGuid().ToString(),
                            false, true, ObjectiveStatus.Failed, false);
                        //addObjectiveMethod.GetValue(bldDestructResult);
                        __instance.AddObjective(bldDestructResult);
                    }
                }

                else if (ModState.HasObjective && ModState.BuildingsDestroyed.Count > 0)
                {
                    if (!ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var count = ModState.BuildingsDestroyedCount;
                        ModInit.modLog.LogMessage($"Total destroyed: {count}");
                        if (count > ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            var diff = count - ModState.CurrentWhiteListInfo.DestructionThreshold;
                            ModInit.modLog.LogMessage($"Diff offset {diff}");
                            foreach (var bldgDestroyed in ModState.BuildingsDestroyed)
                            {
                                ModInit.modLog.LogMessage(
                                    $"Total size {bldgDestroyed.Key} destroyed: {bldgDestroyed.Value.Count}");
                                for (var i = 0; i < diff; i++)
                                {
                                    bldgDestroyed.Value.Count--;
                                    ModInit.modLog.LogMessage($"Decremented destroyed: {bldgDestroyed.Value.Count}");
                                    diff--;
                                    ModInit.modLog.LogMessage($"Decremented diff offset: {diff}");
                                    if (bldgDestroyed.Value.Count < 1 || diff < 1)
                                    {
                                        ModInit.modLog.LogMessage(
                                            $"bldgDestroyed.Value.Count < 1 || diff < 1, breaking loop");
                                        break;
                                    }
                                }

                                if (bldgDestroyed.Value.Count < 1)
                                {
                                    ModInit.modLog.LogMessage(
                                        $"bldgDestroyed.Value.Count < 1, continuing and skipping this size result");
                                    continue;
                                }
                                penalty += bldgDestroyed.Value.BuildingCost * bldgDestroyed.Value.Count;
                            }
                            var bldDestructCost = $"FAILED: Avoid Collateral Damage. {ModState.BuildingsDestroyedCount} buildings destroyed! Fees to be assessed.";
                            var bldDestructResult = new MissionObjectiveResult($"{bldDestructCost}",
                                Guid.NewGuid().ToString(),
                                false, true, ObjectiveStatus.Failed, false);
                            //addObjectiveMethod.GetValue(bldDestructResult);
                            __instance.AddObjective(bldDestructResult);
                        }
                        else if (count <= ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            var bldDestructCost =
                                $"FAILED: Avoid Collateral Damage. {ModState.BuildingsDestroyedCount} Destroyed Buildings < Contracted Limit {ModState.CurrentWhiteListInfo.DestructionThreshold}. No Fees Assessed";
                            var bldDestructResult = new MissionObjectiveResult($"{bldDestructCost}",
                                Guid.NewGuid().ToString(),
                                false, true, ObjectiveStatus.Failed, false);
                                //addObjectiveMethod.GetValue(bldDestructResult);
                            __instance.AddObjective(bldDestructResult);
                        }
                    }

                    if (ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var count = ModState.BuildingsDestroyedCount;
                        ModInit.modLog.LogMessage($"Total destroyed: {count}");
                        if (count > ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            var diff = count - ModState.CurrentWhiteListInfo.DestructionThreshold;
                            ModInit.modLog.LogMessage($"Diff offset {diff}");
                            foreach (var bldgDestroyed in ModState.BuildingsDestroyed)
                            {
                                ModInit.modLog.LogMessage(
                                    $"Total size {bldgDestroyed.Key} destroyed: {bldgDestroyed.Value.Count}");
                                for (var i = 0; i < diff; i++)
                                {
                                    bldgDestroyed.Value.Count--;
                                    ModInit.modLog.LogMessage($"Decremented destroyed: {bldgDestroyed.Value.Count}");
                                    diff--;
                                    ModInit.modLog.LogMessage($"Decremented diff offset: {diff}");
                                    if (bldgDestroyed.Value.Count < 1 || diff < 1)
                                    {
                                        ModInit.modLog.LogMessage(
                                            $"bldgDestroyed.Value.Count < 1 || diff < 1, breaking loop");
                                        break;
                                    }
                                }

                                if (bldgDestroyed.Value.Count < 1)
                                {
                                    ModInit.modLog.LogMessage(
                                        $"bldgDestroyed.Value.Count < 1, continuing and skipping this size result");
                                    continue;
                                }

                                bonus += bldgDestroyed.Value.BuildingCost * bldgDestroyed.Value.Count;
                            }
                            var bldDestructCost =
                                $"SUCCESS: Inflict Collateral Damage. {ModState.BuildingsDestroyedCount} Destroyed Buildings >= Contracted Limit {ModState.CurrentWhiteListInfo.DestructionThreshold}. Bonus to be awarded.";
                            var bldDestructResult = new MissionObjectiveResult($"{bldDestructCost}",
                                Guid.NewGuid().ToString(),
                                false, true, ObjectiveStatus.Succeeded, false);
                            __instance.AddObjective(bldDestructResult);
                        }
                        else if (count <= ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            var bldDestructCost =
                                $"{ModState.BuildingsDestroyedCount} Destroyed Buildings <= Contracted Limit {ModState.CurrentWhiteListInfo.DestructionThreshold}. No Bonus Awarded.";
                            var bldDestructResult = new MissionObjectiveResult($"{bldDestructCost}",
                                Guid.NewGuid().ToString(),
                                false, true, ObjectiveStatus.Failed, false);
                            __instance.AddObjective(bldDestructResult);
                        }
                    }
                }

                if (ModState.HasObjective && ModInit.modSettings.PublicNuisanceDamageOffset > 0f &&
                    ModState.BuildingsDestroyedByOpFor.Count > 0 && !ModState.CurrentWhiteListInfo.DoWarCrimes && penalty > 0f)
                {
                    foreach (var bldgDestroyedOp in ModState.BuildingsDestroyedByOpFor)
                    {
                        mitigation += bldgDestroyedOp.Value.BuildingCost * bldgDestroyedOp.Value.Count * ModInit.modSettings.PublicNuisanceDamageOffset;
                        ModInit.modLog.LogMessage($"Maximum Damage Mitigation: {bldgDestroyedOp.Value.Count} Size {bldgDestroyedOp.Key} Buildings x {bldgDestroyedOp.Value.BuildingCost} ea. Cumulative offset: {mitigation}");
                    }
                }

                var penaltyINT = Mathf.RoundToInt(penalty);
                if (penaltyINT > 0)
                {
                    var finalMitigation = 0;
                    if (mitigation > 0f)
                    {
                        finalMitigation = Mathf.RoundToInt(Mathf.Min(penalty, mitigation));
                        var finalMitigationResult = $"Collateral Damage Fee: ¢{penaltyINT}\nEnemy action negating: ¢{finalMitigation}";
                        var mitigationResult = new MissionObjectiveResult($"{finalMitigationResult}",
                            Guid.NewGuid().ToString(),
                            false, true, ObjectiveStatus.Ignored, false);
                        __instance.AddObjective(mitigationResult);
                    }

                    var netFee = penaltyINT - finalMitigation;
                    var finalPenalty = "";
                    if (netFee > 0)
                    {
                        finalPenalty = $"Final Collateral damage penalties: -¢{netFee}";
                    }
                    else finalPenalty = $"Final Collateral damage penalties: ¢{netFee}";

                    var bldDestructOpResult = new MissionObjectiveResult($"{finalPenalty}",
                        Guid.NewGuid().ToString(),
                        false, true, ObjectiveStatus.Ignored, false);
                    __instance.AddObjective(bldDestructOpResult);
                }

                var bonusINT = Mathf.RoundToInt(bonus);
                if (bonusINT > 0)
                {
                    var finalMitigation = $"Collateral damage bonuses: ¢{bonusINT}";
                    var bldDestructOpResult = new MissionObjectiveResult($"{finalMitigation}",
                        Guid.NewGuid().ToString(),
                        false, true, ObjectiveStatus.Ignored, false);
                    __instance.AddObjective(bldDestructOpResult);
                }
            }
        }


        [HarmonyPatch(typeof(AAR_FactionReputationResultWidget), "FillInData",
            new Type[] { })]
        public static class AAR_FactionReputationResultWidget_FillInData_Patch
        {
            public static void Postfix(AAR_FactionReputationResultWidget __instance)
            {
                var employer = __instance.contract.Override.employerTeam.FactionDef.FactionValue;
                var target = __instance.contract.Override.targetTeam.FactionDef.FactionValue;

                if (ModState.HasObjective && ModState.BuildingsDestroyedCount == 0)
                {
                    if (!ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        if (ModState.CurrentWhiteListInfo.EmployerRepResult != 0 && employer.DoesGainReputation)
                        {
                            var bonusRep = Math.Abs(Mathf.RoundToInt(ModState.CurrentWhiteListInfo.EmployerRepResult *
                                                                     ModState.CurrentWhiteListInfo.DestructionThreshold));
                            var result = __instance.contract.EmployerReputationResults + bonusRep;

                            ModInit.modLog.LogMessage(
                                $"0D_NWC - Original Employer Rep: {__instance.contract.EmployerReputationResults}, Employer BonusRep for no CD: {bonusRep} for total change of {result}");
                            __instance.SetWidgetData(0, employer, result, true);

                            __instance.simState.SetReputation(employer, bonusRep);
                        }

                        if (ModState.CurrentWhiteListInfo.TargetRepResult != 0 && target.DoesGainReputation)
                        {
                            var bonusRep = Math.Abs(Mathf.RoundToInt(ModState.CurrentWhiteListInfo.TargetRepResult *
                                                                     ModState.CurrentWhiteListInfo.DestructionThreshold));
                            var result = __instance.contract.EmployerReputationResults + bonusRep;

                            ModInit.modLog.LogMessage(
                                $"0D_NWC - Original Target Rep: {__instance.contract.TargetReputationResults}, Target BonusRep for no CD: {bonusRep} for total change of {result}");
                            __instance.SetWidgetData(1, target, result, true);

                            __instance.simState.SetReputation(target, bonusRep);
                        }
                    }

                    if (ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        if (ModState.CurrentWhiteListInfo.EmployerRepResult != 0 && employer.DoesGainReputation)
                        {
                            var bonusRep = Math.Abs(Mathf.RoundToInt(ModState.CurrentWhiteListInfo.EmployerRepResult *
                                                                     ModState.CurrentWhiteListInfo.DestructionThreshold));
                            var result = __instance.contract.EmployerReputationResults - bonusRep;

                            ModInit.modLog.LogMessage(
                                $"0D_DWC - Original Employer Rep: {__instance.contract.EmployerReputationResults}, Employer BonusRep for no CD: {-bonusRep} for total change of {result}");
                            __instance.SetWidgetData(0, employer, result, true);

                            __instance.simState.SetReputation(employer, -bonusRep);
                        }

                        if (ModState.CurrentWhiteListInfo.TargetRepResult != 0 && target.DoesGainReputation)
                        {
                            var bonusRep = Math.Abs(Mathf.RoundToInt(ModState.CurrentWhiteListInfo.TargetRepResult *
                                                                     ModState.CurrentWhiteListInfo.DestructionThreshold));
                            var result = __instance.contract.EmployerReputationResults + bonusRep;

                            ModInit.modLog.LogMessage(
                                $"0D_DWC - Original Target Rep: {__instance.contract.TargetReputationResults}, Target BonusRep for no CD: {bonusRep} for total change of {result}");
                            __instance.SetWidgetData(1, target, result, true);

                            __instance.simState.SetReputation(target, bonusRep);
                        }
                    }
                }

                if (ModState.HasObjective && ModState.BuildingsDestroyedCount > 0)
                {
                    var count = ModState.BuildingsDestroyedCount;
                    ModInit.modLog.LogMessage($"Total destroyed: {count}");
                    if (count > ModState.CurrentWhiteListInfo.DestructionThreshold)
                    {
                        var diff = count - ModState.CurrentWhiteListInfo.DestructionThreshold;
                        ModInit.modLog.LogMessage($"Diff offset {diff}");
                        if (diff > 0)
                        {
                            if (!ModState.CurrentWhiteListInfo.DoWarCrimes)
                            {
                                if (ModState.CurrentWhiteListInfo.EmployerRepResult != 0 && employer.DoesGainReputation)
                                {
                                    var bonusRep =
                                        Mathf.RoundToInt(ModState.CurrentWhiteListInfo.EmployerRepResult * diff);
                                    var result = __instance.contract.EmployerReputationResults + bonusRep;

                                    ModInit.modLog.LogMessage(
                                        $"+D_NWC - Original Employer Rep: {__instance.contract.EmployerReputationResults}, Employer Rep Penalty for CD: {bonusRep} for total change of {result}");
                                    __instance.SetWidgetData(0, employer, result, true);

                                    __instance.simState.SetReputation(employer, bonusRep);
                                }

                                if (ModState.CurrentWhiteListInfo.TargetRepResult != 0 && target.DoesGainReputation)
                                {
                                    var bonusRep =
                                        Mathf.RoundToInt(ModState.CurrentWhiteListInfo.TargetRepResult * diff);
                                    var result = __instance.contract.EmployerReputationResults + bonusRep;

                                    ModInit.modLog.LogMessage(
                                        $"+D_NWC - Original Target Rep: {__instance.contract.TargetReputationResults}, Target Rep Penalty for CD: {bonusRep} for total change of {result}");
                                    __instance.SetWidgetData(1, target, result, true);

                                    __instance.simState.SetReputation(target, bonusRep);
                                }
                            }

                            if (ModState.CurrentWhiteListInfo.DoWarCrimes)
                            {
                                if (ModState.CurrentWhiteListInfo.EmployerRepResult != 0 && employer.DoesGainReputation)
                                {
                                    var bonusRep =
                                        Mathf.RoundToInt(ModState.CurrentWhiteListInfo.EmployerRepResult * diff);
                                    var result = __instance.contract.EmployerReputationResults + bonusRep;

                                    ModInit.modLog.LogMessage(
                                        $"+D_DWC - Original Employer Rep: {__instance.contract.EmployerReputationResults}, Employer Rep Penalty for CD: {bonusRep} for total change of {-result}");
                                    __instance.SetWidgetData(0, employer, result, true);

                                    __instance.simState.SetReputation(employer, bonusRep);
                                }

                                if (ModState.CurrentWhiteListInfo.TargetRepResult != 0 && target.DoesGainReputation)
                                {
                                    var bonusRep =
                                        Mathf.RoundToInt(ModState.CurrentWhiteListInfo.TargetRepResult * diff);
                                    var result = __instance.contract.EmployerReputationResults + bonusRep;

                                    ModInit.modLog.LogMessage(
                                        $"+D_DWC - Original Target Rep: {__instance.contract.TargetReputationResults}, Target Rep Penalty for CD: {bonusRep} for total change of {result}");
                                    __instance.SetWidgetData(1, target, result, true);

                                    __instance.simState.SetReputation(target, bonusRep);
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Contract), "CompleteContract", new Type[] {typeof(MissionResult), typeof(bool)})]
        public static class Contract_CompleteContract_Patch
        {
            public static void Postfix(Contract __instance, MissionResult result, bool isGoodFaithEffort)
            {
                var finalDamageCost = 0;
                ModInit.modLog.LogMessage($"CompleteContract: Dumping ModState");
                ModInit.modLog.LogMessage($"CompleteContract: HasObjective:{ModState.HasObjective}");
                ModInit.modLog.LogMessage(
                    $"CompleteContract: BuildingsDestroyed.Count:{ModState.BuildingsDestroyed.Count}");
                ModInit.modLog.LogMessage(
                    $"CompleteContract: BuildingsDestroyedCount:{ModState.BuildingsDestroyedCount}");
                ModInit.modLog.LogMessage(
                    $"CompleteContract: DoesWarCrimes:{ModState.CurrentWhiteListInfo.DoWarCrimes}");
                if (ModState.HasObjective && ModState.BuildingsDestroyed.Count == 0)
                {
                    if (!ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var bonus = 0f;
                        bonus += __instance.InitialContractValue *
                                 ModInit.modSettings.ContractPayFactorBonus;
                        bonus += ModInit.modSettings.FlatRateBonus;

                        var bonusINT = Mathf.RoundToInt(bonus);

                        bonusINT *= ModState.CurrentWhiteListInfo.DestructionThreshold;
                        ModInit.modLog.LogMessage($"CompleteContract 0D_NWC {bonusINT} in collateral damage bonuses!");
                        ModState.FinalPayResult = bonusINT;
                    }

                    if (ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var penalty = 0f;
                        penalty += __instance.InitialContractValue *
                                   ModInit.modSettings.ContractPayFactorBonus;
                        penalty += ModInit.modSettings.FlatRateBonus;

                        var penaltyINT = Mathf.RoundToInt(penalty);
                        penaltyINT *= ModState.CurrentWhiteListInfo.DestructionThreshold;
                        ModInit.modLog.LogMessage(
                            $"CompleteContract 0D_NWC {penaltyINT} in failed collateral damage fees!");

                        finalDamageCost -= penaltyINT;
                    }
                }

                if (ModState.HasObjective && ModState.BuildingsDestroyed.Count == 1 ||
                    ModState.BuildingsDestroyed.FirstOrDefault().Key == 1)
                {
                    if (!ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var destructCostInfo = ModState.BuildingsDestroyed.FirstOrDefault();

                        if (destructCostInfo.Value.Count > ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            var diff = destructCostInfo.Value.Count -
                                       ModState.CurrentWhiteListInfo.DestructionThreshold;
                            var totalCost = destructCostInfo.Value.BuildingCost * diff;

                            var totalCostINT = Mathf.RoundToInt(totalCost);
                            ModInit.modLog.LogMessage(
                                $"CompleteContract: Collateral Damage Fee: {destructCostInfo.Value.Count} Buildings x {destructCostInfo.Value.BuildingCost} ea. = ¢-{totalCostINT}");

                            finalDamageCost -= totalCostINT;
                        }
                        else if (destructCostInfo.Value.Count <= ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            ModInit.modLog.LogMessage(
                                $"CompleteContract: Collateral Damage Fee: {destructCostInfo.Value.Count} Destroyed Buildings < Contracted Limit {ModState.CurrentWhiteListInfo.DestructionThreshold}. No Fees Assessed");
                        }
                    }

                    else if (ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var destructCostInfo = ModState.BuildingsDestroyed.FirstOrDefault();

                        if (destructCostInfo.Value.Count > ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            var diff = destructCostInfo.Value.Count -
                                       ModState.CurrentWhiteListInfo.DestructionThreshold;
                            var totalBonus = destructCostInfo.Value.BuildingCost * diff;

                            var totalBonusINT = Mathf.RoundToInt(totalBonus);
                            ModInit.modLog.LogMessage(
                                $"CompleteContract: Collateral Damage Bonus: {destructCostInfo.Value.Count} Buildings x {destructCostInfo.Value.BuildingCost} ea. = ¢{totalBonusINT}");
                            ModState.FinalPayResult = totalBonusINT;
                        }
                        else if (destructCostInfo.Value.Count <= ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            ModInit.modLog.LogMessage(
                                $"CompleteContract: {destructCostInfo.Value.Count} Destroyed Buildings < Contracted Amount {ModState.CurrentWhiteListInfo.DestructionThreshold}. No Bonus Awarded.");
                        }
                    }
                }

                else if (ModState.HasObjective && ModState.BuildingsDestroyed.Count > 1)
                {

                    if (!ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var count = ModState.BuildingsDestroyedCount;
                        ModInit.modLog.LogMessage($"CompleteContract: Total destroyed: {count}");
                        if (count > ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            var diff = count - ModState.CurrentWhiteListInfo.DestructionThreshold;
                            ModInit.modLog.LogMessage($"CompleteContract: Diff offset {diff}");
                            foreach (var bldgDestroyed in ModState.BuildingsDestroyed)
                            {
                                ModInit.modLog.LogMessage(
                                    $"CompleteContract: Total size {bldgDestroyed.Key} destroyed: {bldgDestroyed.Value.Count}");
                                for (var i = 0; i < diff; i++)
                                {
                                    bldgDestroyed.Value.Count--;
                                    ModInit.modLog.LogMessage(
                                        $"CompleteContract: Decremented destroyed: {bldgDestroyed.Value.Count}");
                                    diff--;
                                    ModInit.modLog.LogMessage($"CompleteContract: Decremented diff offset: {diff}");
                                    if (bldgDestroyed.Value.Count < 1 || diff < 1)
                                    {
                                        ModInit.modLog.LogMessage(
                                            $"CompleteContract: bldgDestroyed.Value.Count < 1 || diff < 1, breaking loop");
                                        break;
                                    }
                                }

                                if (bldgDestroyed.Value.Count < 1)
                                {
                                    ModInit.modLog.LogMessage(
                                        $"CompleteContract: bldgDestroyed.Value.Count < 1, continuing and skipping this size result");
                                    continue;
                                }

                                var totalCost = bldgDestroyed.Value.BuildingCost * bldgDestroyed.Value.Count;
                                finalDamageCost -= totalCost;
                                ModInit.modLog.LogMessage(
                                    $"CompleteContract {totalCost} in collateral damage fees for size {bldgDestroyed.Key}. Current Total Fees: {finalDamageCost}");
                            }
                        }
                    }

                    if (ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var count = ModState.BuildingsDestroyedCount;
                        ModInit.modLog.LogMessage($"CompleteContract: Total destroyed: {count}");
                        if (count > ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            var diff = count - ModState.CurrentWhiteListInfo.DestructionThreshold;
                            ModInit.modLog.LogMessage($"CompleteContract: Diff offset {diff}");
                            foreach (var bldgDestroyed in ModState.BuildingsDestroyed)
                            {
                                ModInit.modLog.LogMessage(
                                    $"CompleteContract: Total size {bldgDestroyed.Key} destroyed: {bldgDestroyed.Value.Count}");
                                for (var i = 0; i < diff; i++)
                                {
                                    bldgDestroyed.Value.Count--;
                                    ModInit.modLog.LogMessage(
                                        $"CompleteContract: Decremented destroyed: {bldgDestroyed.Value.Count}");
                                    diff--;
                                    ModInit.modLog.LogMessage($"CompleteContract: Decremented diff offset: {diff}");
                                    if (bldgDestroyed.Value.Count < 1 || diff < 1)
                                    {
                                        ModInit.modLog.LogMessage(
                                            $"CompleteContract: bldgDestroyed.Value.Count < 1 || diff < 1, breaking loop");
                                        break;
                                    }
                                }

                                if (bldgDestroyed.Value.Count < 1)
                                {
                                    ModInit.modLog.LogMessage(
                                        $"CompleteContract: bldgDestroyed.Value.Count < 1, continuing and skipping this size result");
                                    continue;
                                }

                                var totalBonus = bldgDestroyed.Value.BuildingCost * bldgDestroyed.Value.Count;
                                ModInit.modLog.LogMessage(
                                    $"CompleteContract: Collateral Damage Bonus:  {bldgDestroyed.Value.Count} Size {bldgDestroyed.Key} Buildings > Contracted Limit x {bldgDestroyed.Value.BuildingCost} ea. = ¢{totalBonus}");
                                ModState.FinalPayResult += totalBonus;
                                ModInit.modLog.LogMessage(
                                    $"CompleteContract {totalBonus} in collateral damage bonus for size {bldgDestroyed.Key}. Current Total bonus: {ModState.FinalPayResult}");
                            }
                        }
                    }
                }

                var finalDamageMitigation = 0;
                if (ModState.HasObjective && ModInit.modSettings.PublicNuisanceDamageOffset > 0f &&
                    ModState.BuildingsDestroyedByOpFor.Count > 0 && !ModState.CurrentWhiteListInfo.DoWarCrimes)
                {
                    foreach (var bldgDestroyedOp in ModState.BuildingsDestroyedByOpFor)
                    {
                        var totalOffset = Mathf.RoundToInt(bldgDestroyedOp.Value.BuildingCost *
                                                           bldgDestroyedOp.Value.Count *
                                                           ModInit.modSettings.PublicNuisanceDamageOffset);
                        finalDamageMitigation += totalOffset;
                        ModInit.modLog.LogMessage(
                            $"CompleteContract {totalOffset} in collateral damage mitigation for size {bldgDestroyedOp.Key}. Current Total Offset: {finalDamageMitigation}");

                    }
                }

                var endingCosts = Math.Min(0, finalDamageCost + finalDamageMitigation);
                ModInit.modLog.LogMessage($"Final costs will be {endingCosts}");
                ModInit.modLog.LogMessage($"MoneyResults before penalties: {__instance.MoneyResults}");
                var moneyResults = __instance.MoneyResults + ModState.FinalPayResult + endingCosts;
                ModInit.modLog.LogMessage($"MoneyResults after penalties: {moneyResults}");
                __instance.MoneyResults = moneyResults;//Traverse.Create(__instance).Property("MoneyResults").SetValue(moneyResults);
            }
        }

        [HarmonyPatch(typeof(CombatGameState), "OnCombatGameDestroyed", new Type[] { })]
        public static class CombatGameState_OnCombatGameDestroyed
        {
            public static void Prefix(CombatGameState __instance)
            {
                ModState.Reset();
            }
        }
    }
}