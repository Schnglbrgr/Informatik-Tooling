using UnityEngine.UIElements;


public class ValidationResultElement : VisualElement {

	private readonly Label _severityLabel;
	private readonly Label _ruleLabel;
	private readonly Label _messageLabel;


	public ValidationResultElement() {
		AddToClassList("validation-result");

		_severityLabel = new Label();
		_severityLabel.AddToClassList("validation-result-severity");

		_ruleLabel = new Label();
		_ruleLabel.AddToClassList("validation-result-rule");

		_messageLabel = new Label();
		_messageLabel.AddToClassList("validation-result-message");

		Add(_severityLabel);
		Add(_ruleLabel);
		Add(_messageLabel);
	}


	public void Bind(ValidationResult result) {
		_severityLabel.text = result.Severity.ToString().ToUpper();
		_ruleLabel.text = result.RuleName;
		_messageLabel.text = result.Message;

		RemoveSeverityClasses();

		var severityClass = GetSeverityClass(result.Severity);
		var severityTextClass = GetSeverityTextClass(result.Severity);

		if (!string.IsNullOrEmpty(severityClass))
			AddToClassList(severityClass);

		if (!string.IsNullOrEmpty(severityTextClass))
			_severityLabel.AddToClassList(severityTextClass);
	}


	private void RemoveSeverityClasses() {
		RemoveFromClassList("severity-error");
		RemoveFromClassList("severity-warning");
		RemoveFromClassList("severity-info");

		_severityLabel.RemoveFromClassList("severity-error-text");
		_severityLabel.RemoveFromClassList("severity-warning-text");
		_severityLabel.RemoveFromClassList("severity-info-text");
	}


	private string GetSeverityClass(ValidationSeverity severity) {
		return severity switch {
			ValidationSeverity.Error => "severity-error",
			ValidationSeverity.Warning => "severity-warning",
			ValidationSeverity.Info => "severity-info",
			_ => string.Empty
		};
	}


	private string GetSeverityTextClass(ValidationSeverity severity) {
		return severity switch {
			ValidationSeverity.Error => "severity-error-text",
			ValidationSeverity.Warning => "severity-warning-text",
			ValidationSeverity.Info => "severity-info-text",
			_ => string.Empty
		};
	}

}