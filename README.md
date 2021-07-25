# CollateralDamage

This mod incentivizes players to minimize collateral damage to non-objective, non-enemy buildings. Primarily comes into play in urban settings, but can also apply during "Capture Base" contracts. In vanilla there is nothing that prevents the player from leveling every building in the "base" they're supposed to be capturing and then just occupying the "capture region" and completing the mission. With CollateralDamage, doing so will still complete the mission, but there is a monetary cost associated with every building destroyed.

## Avoid Collateral Damage Objective

New "bonus" objectives can be generated, prompting the player to avoid destroying non-target buidings during contracts. If successful, the player will recieve a bonus; if failed, the player will recieve a penalty for each building destroyed. Bonus objectives are guaranteed for any contracts listed in `WhitelistedContractIDs`, and can optionally be randomly generated according to `CollateralDamageObjectiveChance` (will only be generated on urban maps).

**IMPORTANT** these bonus objectives will show up in the objectives list during a mission, but they are not "real" objectives in the sense that they will not show immediately as "failed" if you destroy a building. Adding new objective logic to handle that would be a ton of work for a teeny payoff. They <i>do</i> however track the number of buildings destroyed and the threshold for receiving penalties/bonuses.

### Collateral Damage

So, what constitutes collateral damage you say? Great question! Collateral damage means destruction of any non-objective, non-enemy buildings.
- Destroy base mission? Those buildings don't count (obviously).
- Playing with Concrete Jungle installed and enemy infantry occupy a highrise? Blast away.
- What about clearing pesky buildings on the way to your objective so you have direct LOS? Yeah... that counts. That's called warcrimes.
- What about stray shots? Those count as well. Gramps always said only shoot at what you can hit, and he was right. Course he also said a bunch of other shit that was completely wrong but you can't win them all.

Settings available:
```
"Settings": {
		"EmployerPlanetsOnly": true,
		"SupportOrAllyCosts": true,
		"SizeFactor": 2.5,
		"FlatRate": 25000,
		"ContractPayFactor": 0.2,
		"PublicNuisanceDamageOffset": 1.0,
		"CollateralDamageObjectiveChance": 1.0,
		"ContractPayFactorBonus": 0.2,
		"FlatRateBonus": 25000,
		"CDThresholdMin": 1,
		"CDThresholdMax": 5,
		"DisableAutoUrbanOnly": true,
		"AllowDisableAutocompleteWhitelist": [
			"DestroyBase_DeniableDestruction"
		],
		"ForceDisableAutocompleteWhitelist": [
			"DestroyBase_Smugglers"
		],
		"WhitelistedContracts": [
			{
				"ContractID": "DestroyBase_DeniableDestruction",
				"DoWarCrimes": true,
				"DestructionThreshold": 3,
				"CBillResultOverride": 0,
				"EmployerRepResult": -5,
				"TargetRepResult": -1
			},
			{
				"ContractID": "DestroyBase_Smugglers",
				"DoWarCrimes": false,
				"DestructionThreshold": 3,
				"CBillResultOverride": 1000.0,
				"EmployerRepResult": -5.0,
				"TargetRepResult": -1.0
			}
		]
	},
```

`EmployerPlanetsOnly` - bool. if true, random objectives will only be generated when the employer is also the system owner.

`SupportOrAllyCosts` - bool. if true, player will be held liable for collateral damage inflicted by allies

`SizeFactor` - float. determines cbill penalty as a multiple of building structure and armor. using above settings and destroying a building with 100 structure, a player would be charged 250 cbills from SizeFactor. stacks with FlatRate and ContractPayFactor.

`FlatRate` - int. flat rate of cbill penalty for each building destroyed. Stacks with SizeFactor and ContractPayFactor.

`ContractPayFactor` - float. determines cbill penalty as a multiple of the **base contract payment value, not the negotiated value.** Using above settings and a contract that would have a base value of 100000, player would be charged 20000 cbills for each building destroyed.

`PublicNuisanceDamageOffset` - float. determines cbill penalty "offset" for buildings destroyed by the OpFor. follows same calculations using SizeFactor, FlatRate, and ContractPayFactor, which are totaled and then multiplied by PublicNuisanceDamageOffset to find the final penalty for a player. Final penalty for player cannot be less than 0. For example, if both the player and the opfor destroy 1 building each of the same size, player would lose 0 cbills **but would fail the bonus objective**. If the player destroyes 1 building, and the opfor destroys 2, the player would still lose 0 cbills, and would still fail the bonus objective.

`CollateralDamageObjectiveChance` - float. probability of contracts on urban maps generating bonus "Avoid collateral damage" objective.

`ContractPayFactorBonus` - float. Similar to ContractPayFactor in that it determines the bonus the player will recieve for completing the "Avoid Collateral Damage" bonus objective as a multiple of the contract base value. Stacks with FlatRateBonus.

`FlatRateBonus` - int. Flat rate of cbill bonus for completing the "Avoid Collateral Damage" bonus objective. Stacks with ContractPayFactorBonus.

`CDThresholdMin` and `CDThresholdMax` - ints. Minimum and maximum bounds for random collateral damage threshold of randomly generated bonus objective. If you destroy non-objective buildings > the chosen threshold, you will begin to amass fees for each building destroyed above the threshold. If you destroy no buildings, the end-of-contract bonus will be `(ContractPayFactorBonus + FlatRateBonus) x threshold`. However, if <i>any</i> buildings are destroyed, but still below the threshold, you recieve no bonus, but no fees.

`"DisableAutoUrbanOnly` - bool. if true, below settings disabling autocomplete only come into play on urban maps.

`AllowDisableAutocompleteWhitelist` - `List<string>`. list of contract IDs for which the player will be given the option to forgo autocompleting when all objectives are met. primarily intended for contracts with DoWarCrimes to enable players to stick around and keep blowing shit up.

`ForceDisableAutocompleteWhitelist` - `List<string>`. list of contract IDs for which autocompletiong will <i>not</i> be allowed. mostly useful on urban maps to allow CJ to keep plinking at players until they evac.

`WhitelistedContracts` - `List<CollateralDamageInfo>`. Information for contracts that will <i>always</i> have the "avoid collateral damage" objective. This also ignores `EmployerPlanetsOnly`. 
	
### Details of new data type `CollateralDamageInfo`:
	
`ContractID` - string. contract ID of whitelisted contract
	
`DoWarCrimes` - bool. if true, player well get <i>paid</i> for destroying non-objective buildings > DestructionThreshold. If no buildings are destroyed, player will pay fee equal to per-building bonus x DestructionThreshold. If false, player will have to pay fee for each building destroyed > DestructionThreshold.

`DestructionThreshold` - int. As described above, threshold of buildings destroyed after which bonuses/fees will be assessed.
	
`CBillResultOverride` - float. if != 0, will override the per-building bonus/fee calculations with this value. if 0, fee/bonus will follow same formula as random objective: `(SizeFactor + FlatRate + ContractPayFactor) x # destroyed > threshold` for fees and `(FlatRate + ContractPayFactor) x # destroyed > threshold` for WarCrimes.	

`EmployerRepResult` and `TargetRepResult` - floats. Per-building reputation change for both employer and target for each building destroyed > threshold. Sign of values is always respected <i>except</i> when no buildings are destroyed. In this case, if DoWarCrimes = false, both are forced positive and player gets reputation bonus of `RepResult x threshold` with employer and target. If DoWarCrimes = true and no buildings are destroyed, player gets reputation bonus with target and reputation penalty with employer.
