using System;
using UnityEngine;


[Serializable]
public class RuleConfiguration {

	[SerializeField] public string ruleId;
	[SerializeField] public bool enabled = true;

	[SerializeField] public SeverityOverrideMode severityOverride = SeverityOverrideMode.UseRuleDefault;

}