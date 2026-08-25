public abstract class ValidationRule {

	public abstract string Id { get; }
	public abstract string Name { get; }
	public virtual string Description => string.Empty;
	
	public abstract ValidationSeverity DefaultSeverity { get; }
	public virtual ValidationCategory Category => ValidationCategory.General;
	protected ValidationSeverity Severity;
	
	public virtual bool IsEnabledByDefault => true;
	public virtual bool CanAutoFix => false;


	public abstract void Validate(ValidationContext context, ValidationResultCollection results);


	public virtual bool TryAutoFix(ValidationContext context, ValidationResult result) {
		return false;
	}


	public void SetSeverity(ValidationSeverity configuredSeverity) {
		Severity = configuredSeverity == ValidationSeverity.UseRuleDefault ? DefaultSeverity : configuredSeverity;
	}


}