using UnityEditor;
using UnityEngine;


[ValidationRule]
public class UnappliedPrefabOverrideRule : ValidationRule {

	public override string Id => "unapplied_prefab_override";
	public override string Name => "Unapplied Prefab Override";
	public override string Description => "Checks the scene for prefab instances with unapplied property overrides.";
	public override string AutoFixDetails => "Reverts all prefab overrides on this instance.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Info;
	public override ValidationCategory Category => ValidationCategory.Prefabs;

	public override bool CanAutoFix => true;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		GameObject[] rootObjects = context.Scene.GetRootGameObjects();

		foreach (GameObject rootObject in rootObjects) {
			ValidateGameObject(rootObject, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		if (PrefabUtility.IsPartOfPrefabInstance(gameObject) && PrefabUtility.HasPrefabInstanceAnyOverrides(gameObject, false)) {
			results.AddResult(Severity, Id, Name, "Unapplied prefab overrides detected.", $"Prefab instance <i><b>{gameObject.name}</b></i> contains unapplied property overrides.",
				gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}


	public override bool TryAutoFix(ValidationContext context, ValidationResult result) {
		if (result.Target is not GameObject gameObject)
			return false;

		if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
			return false;

		if (!PrefabUtility.HasPrefabInstanceAnyOverrides(gameObject, false))
			return false;

		Undo.RegisterFullObjectHierarchyUndo(gameObject, $"Fix {Name}");

		PrefabUtility.RevertPrefabInstance(gameObject, InteractionMode.UserAction);

		return true;
	}

}