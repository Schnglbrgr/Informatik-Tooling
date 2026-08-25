using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;


public static class ValidationRuleRegistry {

	private static readonly Dictionary<string, Type> RulesById = new();
	public static readonly List<Type> RuleTypes = new();


	static ValidationRuleRegistry() {
		Refresh();
	}


	public static ValidationRule CreateRule(string ruleId, ValidationSeverity ruleSeverity) {
		if (!RulesById.TryGetValue(ruleId, out var type))
			return null;

		ValidationRule rule = (ValidationRule)Activator.CreateInstance(type);
		rule.SetSeverity(ruleSeverity);

		return rule;
	}


	public static IReadOnlyList<ValidationRule> CreateRules() {
		return RuleTypes.Select(type => (ValidationRule)Activator.CreateInstance(type)).ToList();
	}


	private static void Refresh() {
		RuleTypes.Clear();
		RulesById.Clear();

		var types = TypeCache.GetTypesDerivedFrom<ValidationRule>();

		foreach (var type in types) {
			if (type.IsAbstract)
				continue;

			if (type.GetCustomAttributes(typeof(ValidationRuleAttribute), true).Length == 0)
				continue;

			RuleTypes.Add(type);

			var rule = (ValidationRule)Activator.CreateInstance(type);
			RulesById[rule.Id] = type;
		}
	}

}