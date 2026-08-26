using UnityEditor;
using UnityEngine;


[ValidationRule]
public class EmptyGameObjectRule : ValidationRule {

	public override string Id => "empty_gameObject";
	public override string Name => "Empty GameObject";
	public override string Description => "Checks the scene for GameObjects that contain no components or children.";
	public override string AutoFixDetails => "Destroys the empty GameObject.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Info;
	public override ValidationCategory Category => ValidationCategory.GameObjects;

	public override bool CanAutoFix => true;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		GameObject[] rootObjects = context.Scene.GetRootGameObjects();

		foreach (GameObject obj in rootObjects) {
			ValidateGameObject(obj, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		if (gameObject.transform.childCount == 0 && gameObject.GetComponents<Component>().Length == 1) {

			results.AddResult(Severity, Id, Name, "Empty GameObject detected.", $"GameObject <i><b>{gameObject.name}</i></b> contains no components and has no children.",
				gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}


	public override bool TryAutoFix(ValidationContext context, ValidationResult result) {
		if (result.Target is not GameObject gameObject)
			return false;

		if (gameObject.transform.childCount > 0 || gameObject.GetComponents<Component>().Length > 1)
			return false;

		Undo.DestroyObjectImmediate(gameObject);

		return true;
	}


}