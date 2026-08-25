public abstract class ValidationRule {

	public abstract string Id { get; }
	public abstract string Name { get; }
	public abstract ValidationSeverity DefaultSeverity { get; }
	public virtual string Description => string.Empty;
	public virtual ValidationCategory Category => ValidationCategory.General;

	protected ValidationSeverity Severity;

	public virtual bool IsEnabledByDefault => true;


	public abstract void Validate(ValidationContext context, ValidationResultCollection results);


	public void SetSeverity(ValidationSeverity configuredSeverity) {
		Severity = configuredSeverity == ValidationSeverity.UseRuleDefault ? DefaultSeverity : configuredSeverity;
	}


}