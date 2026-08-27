using UnityEditor;
using UnityEngine;


[ValidationRule]
public class DisabledBehaviourRule : ValidationRule {

	public override string Id => "disabled_behaviour";
	public override string Name => "Disabled Behaviour";
	public override string Description => "Checks the scene for GameObjects containing disabled Behaviour components.";
	public override string AutoFixDetails => "Enables the disabled Behaviour components on this GameObject.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;
	public override ValidationCategory Category => ValidationCategory.Components;

	public override bool CanAutoFix => true;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		GameObject[] rootObjects = context.Scene.GetRootGameObjects();

		foreach (GameObject obj in rootObjects) {
			ValidateGameObject(obj, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		Component[] components = gameObject.GetComponents<Component>();

		foreach (Component component in components) {
			if (component is Behaviour { enabled: false })
				results.AddResult(Severity, Id, Name, "Disabled Behaviour component detected.",
					$"Component <i><b>{component.GetType().Name}</b></i> on GameObject <i><b>{gameObject.name}</b></i> is disabled.", gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}


	public override bool TryAutoFix(ValidationContext context, ValidationResult result) {
		if (result.Target is not GameObject gameObject)
			return false;

		Undo.RegisterFullObjectHierarchyUndo(gameObject, $"Fix {Name}");

		Component[] components = gameObject.GetComponents<Component>();

		foreach (Component component in components) {
			if (component is Behaviour { enabled: false } behaviour)
				behaviour.enabled = true;
		}

		EditorUtility.SetDirty(gameObject);

		return true;
	}


}