using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.AutoCompleteClasses;
using BattleTech.Framework;
using BattleTech.UI;
using CollateralDamage.Framework;
using Harmony;
using HBS;
using Localize;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CollateralDamage.Patches
{
    class CombatPatches
    {

        // create new objective type and inject it?
        [HarmonyPatch(typeof(TurnDirector), "StartFirstRound")]
        public static class TurnDirector_StartFirstRound
        {
            public static void Postfix(TurnDirector __instance)
            {
                if (ModState.StartFirstRoundOnce) return;
                var sim = UnityGameInstance.BattleTechGame.Simulation;
                if (sim == null) return;
                var contract = __instance.Combat.ActiveContract;

                var whitelistedInfo =
                    ModInit.modSettings.WhitelistedContracts.FirstOrDefault(x => x.ContractID == contract.Override.ID);

                if (whitelistedInfo != null)
                {
                    ModState.CurrentWhiteListInfo = whitelistedInfo;
                    ModInit.modLog.LogMessage(
                        $"Contract Whitelisted, processing settings and creating secondary objective.");
                    
                    ModState.HasObjective = true;

                    var HUD = Traverse.Create(CameraControl.Instance).Property("HUD").GetValue<CombatHUD>();
                    var Notify = Traverse.Create(HUD).Property("ObjectiveStatusNotify").GetValue<CombatHUDObjectiveStatusNotify>();
                    var objectivesList = HUD.ObjectivesList;
                    ModInit.modLog.LogMessage(
                        $"Collateral Damage Objective Added to objectives");
                    var objectiveUIItem =
                        UnityEngine.Object.Instantiate<CombatHUDObjectiveItem>(objectivesList.secondaryObjectivePrefab);
                    objectiveUIItem.transform.SetParent(objectivesList.objectivesStack.transform);
                    objectiveUIItem.transform.localScale = Vector3.one;
                    var objectiveUIList = Traverse.Create(objectivesList).Field("objectiveUIItems")
                        .GetValue<List<CombatHUDObjectiveItem>>();
                    Traverse.Create(objectiveUIItem).Field("notify").SetValue(Notify);
                    objectiveUIList.Add(objectiveUIItem);

                    if (whitelistedInfo.DoWarCrimes)
                    {
                        var text = new Text($"Inflict Collateral Damage! Destroy 1 extra to avoid penalty, and at least {ModState.CurrentWhiteListInfo.DestructionThreshold} buildings to receive bonus.");
                        objectiveUIItem.Init(new Text(text), false, false);
                        var progressText = new Text($"Current: {ModState.BuildingsDestroyedCount}/{ModState.CurrentWhiteListInfo.DestructionThreshold}");
                        Util._SetProgressText.Invoke(objectiveUIItem, new object[] {progressText});

                    }
                    else
                    {
                        var text = new Text($"Avoid Collateral Damage! Destroy fewer than {ModState.CurrentWhiteListInfo.DestructionThreshold} buildings to avoid penalties. Destroy 0 for bonus!");
                        objectiveUIItem.Init(new Text(text), false, false);
                        var progressText = new Text($"Current: {ModState.BuildingsDestroyedCount}/{ModState.CurrentWhiteListInfo.DestructionThreshold}");
                        Util._SetProgressText.Invoke(objectiveUIItem, new object[] { progressText });
                    }

                    ModInit.modLog.LogMessage(
                        $"Collateral Damage objectiveUIItem initialized");
                    objectiveUIItem.SetStatusColors(objectivesList.objectiveSucceeded.color,
                        objectivesList.objectiveFailed.color);
                    objectiveUIItem.gameObject.SetActive(true);
                    ModInit.modLog.LogMessage(
                        $"Collateral Damage objectiveUIItem set active");
                    ModState.StartFirstRoundOnce = true;
                    return;
                }
                else
                {
                    ModState.CurrentWhiteListInfo = new CollateralDamageInfo {ContractID = contract.Override.ID, EmployerRepResult = ModInit.modSettings.EmployerRepResult, TargetRepResult =  ModInit.modSettings.TargetRepResult};
                }

                var roll = ModInit.Random.NextDouble();

                if (__instance.Combat.MapMetaData.biomeSkin != Biome.BIOMESKIN.urbanHighTech && contract.Override.ContractTypeValue.Name != "CaptureBase")
                {
                    ModState.StartFirstRoundOnce = true;
                    return;
                }
                if (roll <= ModInit.modSettings.CollateralDamageObjectiveChance)
                {

                    if (ModInit.modSettings.EmployerPlanetsOnly && contract.Override.employerTeam.FactionValue.Name !=
                        sim.CurSystem.OwnerValue.Name)
                    {
                        var warCrimesRoll = ModInit.Random.NextDouble();
                        if (warCrimesRoll <= ModInit.modSettings.DoWarCrimesChance && contract.Override.ContractTypeValue.Name != "CaptureBase")
                        {
                            ModInit.modLog.LogMessage($"Roll {warCrimesRoll} <= threshold {ModInit.modSettings.DoWarCrimesChance}, setting DoWarCrimes true");
                            ModState.CurrentWhiteListInfo.DoWarCrimes = true;
                        }
                        else
                        {
                            ModInit.modLog.LogMessage($"Roll {warCrimesRoll} > threshold {ModInit.modSettings.DoWarCrimesChance}, no warcrimes, no objective.");
                            ModState.StartFirstRoundOnce = true;
                            return;
                        }
                    }

                    ModInit.modLog.LogMessage(
                        $"Roll {roll} <= threshold {ModInit.modSettings.CollateralDamageObjectiveChance}, creating secondary objective. Is DoWarCrimes?: {ModState.CurrentWhiteListInfo.DoWarCrimes}");

                    ModState.HasObjective = true;
                    var threshold = Random.Range(ModInit.modSettings.CDThresholdMin,
                        ModInit.modSettings.CDThresholdMax + 1);
                    ModState.CurrentWhiteListInfo.DestructionThreshold = threshold;

                    var cap = Random.Range(threshold + ModInit.modSettings.CDCapMin,
                        threshold + ModInit.modSettings.CDCapMax + 1);
                    ModState.CurrentWhiteListInfo.DestructionCap = cap;

                    var HUD = Traverse.Create(CameraControl.Instance).Property("HUD").GetValue<CombatHUD>();
                    var Notify = Traverse.Create(HUD).Property("ObjectiveStatusNotify").GetValue<CombatHUDObjectiveStatusNotify>();
                    var objectivesList = HUD.ObjectivesList;
                    ModInit.modLog.LogMessage(
                        $"Collateral Damage Objective Added to objectives");
                    var objectiveUIItem =
                        UnityEngine.Object.Instantiate<CombatHUDObjectiveItem>(objectivesList.secondaryObjectivePrefab);
                    objectiveUIItem.transform.SetParent(objectivesList.objectivesStack.transform);
                    objectiveUIItem.transform.localScale = Vector3.one;
                    var objectiveUIList = Traverse.Create(objectivesList).Field("objectiveUIItems")
                        .GetValue<List<CombatHUDObjectiveItem>>();
                    Traverse.Create(objectiveUIItem).Field("notify").SetValue(Notify);
                    objectiveUIList.Add(objectiveUIItem);

                    if (ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var text = new Text($"Inflict Collateral Damage! Destroy 1 extra to avoid penalty, and at least {ModState.CurrentWhiteListInfo.DestructionThreshold} buildings to receive bonus.");
                        objectiveUIItem.Init(new Text(text), false, false);
                        var progressText = new Text($"Current: {ModState.BuildingsDestroyedCount}/{ModState.CurrentWhiteListInfo.DestructionThreshold}");
                        Util._SetProgressText.Invoke(objectiveUIItem, new object[] { progressText });

                    }
                    else
                    {
                        var text = new Text($"Avoid Collateral Damage! Destroy fewer than {ModState.CurrentWhiteListInfo.DestructionThreshold} buildings to avoid penalties. Destroy 0 for bonus!");
                        objectiveUIItem.Init(new Text(text), false, false);
                        var progressText = new Text($"Current: {ModState.BuildingsDestroyedCount}/{ModState.CurrentWhiteListInfo.DestructionThreshold}");
                        Util._SetProgressText.Invoke(objectiveUIItem, new object[] { progressText });
                    }

                    ModInit.modLog.LogMessage(
                        $"Collateral Damage objectiveUIItem initialized");
                    objectiveUIItem.SetStatusColors(objectivesList.objectiveSucceeded.color,
                        objectivesList.objectiveFailed.color);
                    objectiveUIItem.gameObject.SetActive(true);
                    ModInit.modLog.LogMessage(
                        $"Collateral Damage objectiveUIItem set active");
                    ModState.StartFirstRoundOnce = true;
                }
                else
                {
                    ModInit.modLog.LogMessage(
                        $"Contract not whitelisted or roll {roll} > threshold {ModInit.modSettings.CollateralDamageObjectiveChance}. No secondary objective will be created, no collateral damage will be penalized.");
                    ModState.StartFirstRoundOnce = true;
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

                var attackingUnit = __instance.Combat.FindActorByGUID(attackerID);

                if (ModInit.modSettings.PublicNuisanceDamageOffset > 0f || !ModState.CurrentWhiteListInfo.DoWarCrimes)
                {
                    if (attackingUnit == null || // null attacker should be CJ blowing things up with dummy GUID, proc as offset.
                        attackingUnit.team.GUID == "be77cadd-e245-4240-a93e-b99cc98902a5" || // Target
                        attackingUnit.team.GUID == "31151ed6-cfc2-467e-98c4-9ae5bea784cf" || // TargetsAlly
                        attackingUnit.team.GUID == "3c9f3a20-ab03-4bcb-8ab6-b1ef0442bbf0") // HostileToAll
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
                        if (ModState.CurrentWhiteListInfo.CBillResultOverride != 0)
                        {
                            totalCost = ModState.CurrentWhiteListInfo.CBillResultOverride;
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
                        return;
                    }
                }

                if (attackingUnit == null)
                {
                    ModInit.modLog.LogMessage(
                        $"Couldn't find attackingUnit. Probably ConcreteJungle dummy GUID.");
                    return;
                }

                if (!ModInit.modSettings.SupportOrAllyCosts &&
                    attackingUnit.team.GUID == "9ed02e70-beff-4131-952e-49d366e2f7cc" || // PlayerOneSupport
                    attackingUnit.team.GUID == "70af7e7f-39a8-4e81-87c2-bd01dcb01b5e") return;  // EmployersAlly
                   // !attackingUnit.team.IsLocalPlayer) return;
                //if (!ModInit.modSettings.SupportOrAllyCosts && !attackingUnit.team.IsLocalPlayer) return;

                if (attackingUnit.team.IsLocalPlayer) // HostileToAll
                {
                    if (ModState.CurrentWhiteListInfo.DestructionCap > 0 && ModState.BuildingsDestroyedCount >= ModState.CurrentWhiteListInfo.DestructionCap) return;
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

                    if (ModState.CurrentWhiteListInfo.CBillResultOverride != 0)
                    {
                        totalCost = ModState.CurrentWhiteListInfo.CBillResultOverride;
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

                    ModState.BuildingsDestroyedCount += 1;

                    var HUD = Traverse.Create(CameraControl.Instance).Property("HUD").GetValue<CombatHUD>();
                    var objectivesList = HUD.ObjectivesList;
                    var objectiveUIList = Traverse.Create(objectivesList).Field("objectiveUIItems")
                        .GetValue<List<CombatHUDObjectiveItem>>();

                    var objectiveAVOID =
                        objectiveUIList.FirstOrDefault(x => x.ObjectiveText.text.StartsWith("Avoid Collateral Damage!"));
                    if (objectiveAVOID != null)
                    {
                        if (!ModState.CurrentWhiteListInfo.DoWarCrimes && ModState.BuildingsDestroyedCount >
                            ModState.CurrentWhiteListInfo.DestructionThreshold)
                        {
                            var objectiveUIItem =
                                UnityEngine.Object.Instantiate<CombatHUDObjectiveItem>(objectivesList.secondaryObjectivePrefab);
                            objectiveUIItem.transform.SetParent(objectivesList.objectivesStack.transform);
                            objectiveUIItem.transform.localScale = Vector3.one;
                            objectiveUIList.Add(objectiveUIItem);
                            objectiveAVOID.gameObject.SetActive(false);
                            ModInit.modLog.LogMessage(
                                $"Avoid Collateral Damage FAILED. Original Objective Set Inactive");

                            objectiveUIItem.Init(new Text("FAILED: Avoid Collateral Damage"), false, false);
                            //objectiveUIItem.Init(__instance.Combat, HUD, objectivesList, objectivesNotify, newObjectiveLogic);
                            ModInit.modLog.LogMessage(
                                $"Collateral Damage objectiveUIItem initialized");
                            objectiveUIItem.SetStatusColors(objectivesList.objectiveFailed.color,
                                objectivesList.objectiveSucceeded.color);
                            objectiveUIItem.ObjectiveCheckedPip.vectorGraphics = objectiveUIItem.ObjectiveFailedPip;
                            HUD.PlayAudioEvent(AudioEventList_ui.ui_objective_fail);

                            objectiveUIItem.gameObject.SetActive(true);
                            var progressText = new Text($"Current: {ModState.BuildingsDestroyedCount}/{ModState.CurrentWhiteListInfo.DestructionThreshold}");
                            Util._SetProgressText.Invoke(objectiveAVOID, new object[] { progressText });
                            ModInit.modLog.LogMessage(
                                $"Collateral Damage objectiveUIItem FAILED set active");
                        }
                    }

                    if (ModState.CurrentWhiteListInfo.DoWarCrimes)
                    {
                        var objectiveINFLICT =
                            objectiveUIList.FirstOrDefault(
                                x => x.ObjectiveText.text.StartsWith("Inflict Collateral Damage!"));
                        if (objectiveINFLICT != null)
                        {
                            //var text = new Text($"Inflict Collateral Damage! Destroy 1 extra to avoid penalty, and at least{ModState.CurrentWhiteListInfo.DestructionThreshold} buildings to receive bonus. Current {ModState.BuildingsDestroyedCount}/{ModState.CurrentWhiteListInfo.DestructionThreshold}");

                            var text = new Text(
                                $"Inflict Collateral Damage! Destroy 1 extra to avoid penalty, and at least {ModState.CurrentWhiteListInfo.DestructionThreshold} buildings to receive bonus.");

                            var progressText = new Text($"Current: {ModState.BuildingsDestroyedCount}/{ModState.CurrentWhiteListInfo.DestructionThreshold}");
                            Util._SetProgressText.Invoke(objectiveINFLICT, new object[] { progressText });
                            objectiveINFLICT.ObjectiveText.SetText(text);
                            ModInit.modLog.LogMessage(
                                $"Collateral Damage objectiveUIItem SUCCESS set active");
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AutoCompleteGameLogic), "CheckAutoComplete", new Type[] {})]
        public static class AutoCompleteGameLogic_CheckAutoComplete
        {
            public static bool Prefix(AutoCompleteGameLogic __instance)
            {
                if (__instance == null)
                {
                    ModInit.modLog.LogMessage(
                        $"instance was null? wtf.");
                    return true;
                }

                if (ModInit.modSettings.DisableAutoUrbanOnly &&
                    __instance.Combat.MapMetaData.biomeSkin != Biome.BIOMESKIN.urbanHighTech) return true;

                if (__instance.Combat.RegionsList.All(
                    x => x.regionDefId != "regionDef_EvacZone") || ModState.HasSeenEvacPopup || (!ModInit.modSettings.AllowDisableAutocompleteWhitelist.Contains(__instance.Combat.ActiveContract.Override.ID) && !ModInit.modSettings.ForceDisableAutocompleteWhitelist.Contains(__instance.Combat.ActiveContract.Override.ID)))
                {
                    return true;
                }
                __instance.checkAutoCompleteFlag = false;
                if (TriggeringObjectiveStatus.CheckTriggeringObjectiveList(__instance, __instance.DisplayName, __instance.triggeringObjectiveList, __instance.Combat))
                {
                    if (ModInit.modSettings.AllowDisableAutocompleteWhitelist.Contains(__instance.Combat.ActiveContract.Override.ID))
                    {
                        var popup = GenericPopupBuilder
                            .Create("Call For Pickup",
                                "Sumire can pick you up from your current location, or you can proceed to the evac point.")
                            .AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants
                                .PopupBackfill));
                        popup.AlwaysOnTop = true;
                        popup.AddButton("Proceed to Evac Zone.", () =>
                        {
                            ModInit.modLog.LogMessage(
                                $"Selected Proceed to Evac Zone.");
                        });
                        popup.AddButton("Extract Immediately.", () =>
                        {
                            
                            ModInit.modLog.LogMessage(
                                $"Selected extract immediately, autocompleting.");
                            Traverse.Create(__instance).Method("AutoComplete").GetValue();
                        });
                        popup.Render();
                        ModState.HasSeenEvacPopup = true;
                    }
                    else if (ModInit.modSettings.ForceDisableAutocompleteWhitelist.Contains(__instance.Combat.ActiveContract.Override.ID))
                    {
                        return false;
                    }
                }
                return false;
            }
        }
    }
}
