using System.Collections.Generic;
using UnityEngine;


public sealed class ValidationResultCollection {

	private readonly List<ValidationResult> _results = new();

	public IList<ValidationResult> Results => _results;


	public void Add(ValidationResult result) {
		_results.Add(result);
	}


	public void Info(string ruleId, string ruleName, string message, string details, Object target = null, bool canAutoFix = false) {
		Add(new ValidationResult(ValidationSeverity.Info, ruleId, ruleName, message, details, target, canAutoFix));
	}


	public void Warning(string ruleId, string ruleName, string message, string details, Object target = null, bool canAutoFix = false) {
		Add(new ValidationResult(ValidationSeverity.Warning, ruleId, ruleName, message, details, target, canAutoFix));
	}


	public void Error(string ruleId, string ruleName, string message, string details, Object target = null, bool canAutoFix = false) {
		Add(new ValidationResult(ValidationSeverity.Error, ruleId, ruleName, message, details, target, canAutoFix));
	}


}