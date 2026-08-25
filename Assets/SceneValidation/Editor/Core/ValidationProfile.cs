using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ValidationProfile", menuName = "Scene Validation/Validation Profile")]
public class ValidationProfile : ScriptableObject {

	[SerializeField] private List<RuleConfiguration> ruleConfigurations = new();

	public IReadOnlyList<RuleConfiguration> RuleConfigurations => ruleConfigurations;


	public void ResetToDefaultRules() {
		ruleConfigurations.Clear();

		foreach (var rule in ValidationRuleRegistry.CreateRules()) {
			ruleConfigurations.Add(new RuleConfiguration { ruleId = rule.Id, enabled = rule.IsEnabledByDefault, severityOverride = rule.DefaultSeverity });
		}
	}


	public bool HasRule(string ruleId) {
		foreach (var rule in ruleConfigurations) {
			if (rule.ruleId == ruleId)
				return true;
		}

		return false;
	}


	public void AddRule(string ruleId) {
		if (HasRule(ruleId))
			return;

		ruleConfigurations.Add(new RuleConfiguration { ruleId = ruleId, enabled = true, severityOverride = ValidationSeverity.UseRuleDefault });
	}


	public void RemoveRule(string ruleId) {
		ruleConfigurations.RemoveAll(x => x.ruleId == ruleId);
	}

}