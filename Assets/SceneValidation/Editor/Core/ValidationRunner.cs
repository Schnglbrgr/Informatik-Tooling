public sealed class ValidationRunner {

	public ValidationResultCollection Validate(ValidationContext context) {
		var results = new ValidationResultCollection();
		var profile = context.Profile;

		if (!profile) {
			results.Error("profile_configuration", "Invalid Validation Profile", "No validation profile was provided.",
				"Select a validation profile before validating the scene.");

			return results;
		}

		if (profile.Rules == null || profile.Rules.Count == 0) {
			results.Error("profile_configuration", "Empty Validation Profile", "The validation profile contains no rules.",
				"Add at least one validation rule to the profile.");

			return results;
		}


		foreach (var configuration in profile.Rules) {
			if (!configuration.enabled)
				continue;

			if (string.IsNullOrWhiteSpace(configuration.ruleId)) {
				results.Error("profile_configuration", "Invalid Rule Configuration", "A validation rule has no Rule ID.",
					"Every rule configuration must reference a valid rule.");

				continue;
			}

			var rule = ValidationRuleRegistry.CreateRule(configuration.ruleId);

			if (rule == null) {
				results.Error("profile_configuration", "Unknown Validation Rule", $"Rule '{configuration.ruleId}' could not be found.",
					"The validation profile references a rule that is not registered.");

				continue;
			}

			rule.Validate(context, results);
		}

		return results;
	}

}