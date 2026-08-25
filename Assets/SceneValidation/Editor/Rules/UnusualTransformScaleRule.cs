using UnityEngine;


[ValidationRule]
public sealed class UnusualTransformScaleRule : ValidationRule {

	public override string Id => "unusual_transform_scale";

	public override string Name => "Unusual Transform Scale";
	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Info;

	public override string Description => "Checks for unusually large or negative scales.";

	public override ValidationCategory Category => ValidationCategory.GameObjects;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		foreach (var root in context.Scene.GetRootGameObjects()) {
			ValidateGameObject(root, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		var scale = gameObject.transform.localScale;

		if (scale.x > 10f || scale.y > 10f || scale.z > 10f) {
			results.AddResult(Severity, Id, Name, $"GameObject <i><b>'{gameObject.name}'</i></b> has an unusual scale.", $"Current scale: {scale}.", gameObject);
		}

		if (scale.x < 0 || scale.y < 0 || scale.z < 0) {
			results.AddResult(Severity, Id, Name, $"GameObject <i><b>'{gameObject.name}'</i></b> has negative scale.", "Current scale: {scale}.", gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}

}