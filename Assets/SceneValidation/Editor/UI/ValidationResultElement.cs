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
		_severityLabel.text = GetSeverityText(result.Severity);
		_ruleLabel.text = result.RuleName;
		_messageLabel.text = result.Message;

		RemoveFromClassList("severity-error");
		RemoveFromClassList("severity-warning");
		RemoveFromClassList("severity-info");

		AddToClassList(GetSeverityClass(result.Severity));
	}


	private string GetSeverityText(ValidationSeverity severity) {
		return severity switch {
			ValidationSeverity.Error => "ERROR",
			ValidationSeverity.Warning => "WARNING",
			ValidationSeverity.Info => "INFO",
			_ => "INFO"
		};
	}


	private string GetSeverityClass(ValidationSeverity severity) {
		return severity switch {
			ValidationSeverity.Error => "severity-error",
			ValidationSeverity.Warning => "severity-warning",
			ValidationSeverity.Info => "severity-info",
			_ => "severity-info"
		};
	}

}