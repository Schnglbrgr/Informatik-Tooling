using UnityEngine;


[ValidationRule]
public class RigidbodyColliderRule : ValidationRule {

	public override string Id => "rigidbody_collider";
	public override string Name => "Rigidbody Without Collider";
	public override string Description => "Checks the scene for Rigidbodies without a Collider.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;
	public override ValidationCategory Category => ValidationCategory.Physics;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		GameObject[] rootObjects = context.Scene.GetRootGameObjects();

		foreach (GameObject obj in rootObjects) {
			ValidateGameObject(obj, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

		if (rigidbody && !gameObject.GetComponent<Collider>()) {
			results.AddResult(Severity, Id, Name, "Rigidbody without Collider.",
				$"GameObject <i><b>{gameObject.name}</b></i> contains a Rigidbody but no Collider. This may cause unexpected physics behaviour.", gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}

}