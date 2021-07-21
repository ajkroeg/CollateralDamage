# CollateralDamage

This mod incentivizes players to minimize collateral damage to non-objective, non-enemy buildings. Primarily comes into play in urban settings, but can also apply during "Capture Base" contracts. In vanilla there is nothing that prevents the player from leveling every building in the "base" they're supposed to be capturing and then just occupying the "capture region" and completing the mission. With CollateralDamage, doing so will still complete the mission, but there is a monetary cost associated with every building destroyed.

## Avoid Collateral Damage Objective

New "bonus" objectives can be generated, prompting the player to avoid destroying non-target buidings during contracts. If successful, the player will recieve a bonus; if failed, the player will recieve a penalty for each building destroyed. Bonus objectives are guaranteed for any contracts listed in `WhitelistedContractIDs`, and can optionally be randomly generated according to `CollateralDamageObjectiveChance` (will only be generated on urban maps).

**IMPORTANT** these bonus objectives will show up in the objectives list during a mission, but they are not "real" objectives in the sense that they will not show immediately as "failed" if you destroy a building. Adding new objective logic to handle that would be a ton of work for a teeny payoff.

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
		"WhitelistedContractIDs": []
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

`WhitelistedContractIDs` - List<string>. List of contract IDs that will <i>always</i> have the "avoid collateral damage" objective. Primarily used for capture base.
