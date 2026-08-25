using UnityEngine;


public sealed class ValidationResult {

	public ValidationSeverity Severity { get; }
	public string RuleId { get; }
	public string RuleName { get; }
	public string Message { get; }
	public string Details { get; }
	public Object Target { get; }

	public bool CanAutoFix { get; }


	public ValidationResult(ValidationSeverity severity, string ruleId, string ruleName, string message, string details, Object target = null,
		bool canAutoFix = false) {
		Severity = severity;
		RuleId = ruleId;
		RuleName = ruleName;
		Message = message;
		Details = details;
		Target = target;
		CanAutoFix = canAutoFix;
	}

}