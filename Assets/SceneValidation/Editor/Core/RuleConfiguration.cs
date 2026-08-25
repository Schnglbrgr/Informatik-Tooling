using System;
using UnityEngine;


[Serializable]
public class RuleConfiguration {

	[SerializeField] public string ruleId;
	[SerializeField] public bool enabled = true;

	[SerializeField] public ValidationSeverity severityOverride = ValidationSeverity.UseRuleDefault;

}