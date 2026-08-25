using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ValidationProfile", menuName = "Scene Validation/Validation Profile")]
public class ValidationProfile : ScriptableObject {

	[SerializeField] private List<RuleConfiguration> rules = new();

	public IReadOnlyList<RuleConfiguration> Rules => rules;


	public void ResetToDefaultRules() {
		rules.Clear();

		foreach (var rule in ValidationRuleRegistry.CreateRules()) {
			rules.Add(new RuleConfiguration {
				ruleId = rule.Id, enabled = rule.IsEnabledByDefault, severityOverride = SeverityOverrideMode.UseRuleDefault
			});
		}
	}


	public bool HasRule(string ruleId) {
		foreach (var rule in rules) {
			if (rule.ruleId == ruleId)
				return true;
		}

		return false;
	}


	public void AddRule(string ruleId) {
		if (HasRule(ruleId))
			return;

		rules.Add(new RuleConfiguration { ruleId = ruleId, enabled = true, severityOverride = SeverityOverrideMode.UseRuleDefault });
	}


	public void RemoveRule(string ruleId) {
		rules.RemoveAll(x => x.ruleId == ruleId);
	}

}