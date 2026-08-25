public abstract class ValidationRule {

	public abstract string Id { get; }
	public abstract string Name { get; }

	public virtual string Description => string.Empty;

	public virtual ValidationCategory Category => ValidationCategory.General;

	public virtual bool IsEnabledByDefault => true;


	public abstract void Validate(ValidationContext context, ValidationResultCollection results);

}