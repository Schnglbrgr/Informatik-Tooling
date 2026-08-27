using UnityEditor;
using UnityEngine;


[ValidationRule]
public sealed class DisabledGameObjectRule : ValidationRule {

	public override string Id => "disabled_gameObject";
	public override string Name => "Disabled GameObject";
	public override string Description => "Checks the scene for disabled GameObjects.";
	public override string AutoFixDetails => "Activates the disabled GameObject.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;
	public override ValidationCategory Category => ValidationCategory.GameObjects;
	public override bool CanAutoFix => true;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		foreach (GameObject root in context.Scene.GetRootGameObjects()) {
			ValidateGameObject(root, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		if (!gameObject.activeSelf) {
			results.AddResult(Severity, Id, Name, $"GameObject <i><b>{gameObject.name}</b></i> is disabled.", "This GameObject is currently inactive in the scene.", gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}


	public override bool TryAutoFix(ValidationContext context, ValidationResult result) {
		if (result.Target is not GameObject gameObject)
			return false;

		Undo.RecordObject(gameObject, $"Fix {Name}");

		gameObject.SetActive(true);

		EditorUtility.SetDirty(gameObject);

		return true;
	}

}