using UnityEngine;


[ValidationRule]
public sealed class DisabledGameObjectRule : ValidationRule {

	public override string Id => "disabled_gameObject";
	public override string Name => "Disabled GameObject";
	public override string Description => "Checks the scene for disabled GameObjects.";

	public override ValidationCategory Category => ValidationCategory.GameObjects;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		foreach (GameObject root in context.Scene.GetRootGameObjects()) {
			ValidateGameObject(root, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		if (!gameObject.activeSelf) {
			results.Warning(Id, Name, $"GameObject '{gameObject.name}' is disabled.", "This GameObject is currently inactive in the scene.",
				gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}

}