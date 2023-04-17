using System;
using System.Collections.Generic;
using System.Reflection;
using CollateralDamage.Framework;
using Newtonsoft.Json;
using HarmonyLib;

namespace CollateralDamage
{
    public static class ModInit
    {
        internal static Logger modLog;
        internal static string modDir;
        internal static Settings modSettings;
        public static readonly Random Random = new Random();
        public const string HarmonyPackage = "us.tbone.CollateralDamage";
        public static void Init(string directory, string settingsJSON)
        {
            modDir = directory;
            modLog = new Logger(modDir, "CollateralDamage", true);
            try
            {
                ModInit.modSettings = JsonConvert.DeserializeObject<Settings>(settingsJSON);

            }
            catch (Exception ex)
            {
                ModInit.modLog.LogException(ex);
                ModInit.modSettings = new Settings();
            }


            ModInit.modLog.LogMessage($"Initializing {HarmonyPackage} - Version {typeof(Settings).Assembly.GetName().Version}");
            Util.LogSettings();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), HarmonyPackage);
        }
    }
    class Settings
    {
        public bool EmployerPlanetsOnly = false; // damages only assessed when employer owns planet

        public bool SupportOrAllyCosts = true; // player is liable for damages done by their allies

        public float SizeFactor = 0f; // 0 disables Size Factor - multiplies with building structure+armor, stacks with FlatRate and ContractPayFactor

        public int FlatRate = 25000; // 0 disables flat rate, stacks with PayFactor and SizeFactor

        public float ContractPayFactorDmg = 0.1f; // 0 disables PayFactor, stacks with FlatRate and SizeFactor

        public float PublicNuisanceDamageOffset = 1f; // 0 disables "offsetting" of player collateral damage fees by destruction wrought by target team

        public float CollateralDamageObjectiveChance = 0.5f; // chance for contract to randomly have "avoid collateral damage" as an objective. whitelisted contracts always have the objective.

        public float ContractPayFactorBonus = 0.1f; // 0 disables PayBonusFactor, stacks with FlatRate;

        public int FlatRateBonus = 25000; // 0 disables FlatRateBonus, stacks with PayBonusFactor;

        public int CDThresholdMin = 1;
        public int CDCapMin = 0;
        public int CDThresholdMax = 10;
        public int CDCapMax = 0;

        public float EmployerRepResult = 0f;
        public float TargetRepResult = 0f;

        public float DoWarCrimesChance = 0f;

        public List<CollateralDamageInfo> WhitelistedContracts = new List<CollateralDamageInfo>(); // damages on these contracts

        public bool DisableAutoUrbanOnly = true;
        public List<string> AllowDisableAutocompleteWhitelist = new List<string>();
        public List<string> ForceDisableAutocompleteWhitelist = new List<string>();
    }
}