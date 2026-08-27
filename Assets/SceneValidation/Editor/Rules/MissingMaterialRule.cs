using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[ValidationRule]
public class MissingMaterialRule : ValidationRule {

	public override string Id => "missing_material";
	public override string Name => "Missing Material";
	public override string Description => "Checks the scene for renderers with missing materials.";
	public override string AutoFixDetails => "Removes empty material slots from the renderer.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;
	public override ValidationCategory Category => ValidationCategory.Rendering;

	public override bool CanAutoFix => true;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		GameObject[] rootObjects = context.Scene.GetRootGameObjects();

		foreach (GameObject obj in rootObjects) {
			ValidateGameObject(obj, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		if (gameObject.TryGetComponent(out Renderer renderer)) {
			Material[] materials = renderer.sharedMaterials;

			for (int i = 0; i < materials.Length; i++) {
				if (materials[i])
					continue;

				results.AddResult(Severity, Id, Name, "Missing material detected.",
					$"Renderer <i><b>{renderer.GetType().Name}</b></i> on GameObject <i><b>{gameObject.name}</b></i> has no material assigned to material slot <i><b>{i}</b></i>.",
					gameObject);
			}
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}


	public override bool TryAutoFix(ValidationContext context, ValidationResult result) {
		if (result.Target is not GameObject gameObject)
			return false;

		if (!gameObject.TryGetComponent(out Renderer renderer))
			return false;

		Material[] materials = renderer.sharedMaterials;

		if (materials.Length == 0)
			return false;

		Undo.RecordObject(renderer, $"Fix {Name}");

		var validMaterials = new List<Material>();

		foreach (Material material in materials) {
			if (material)
				validMaterials.Add(material);
		}

		if (validMaterials.Count == materials.Length)
			return false;

		renderer.sharedMaterials = validMaterials.ToArray();

		EditorUtility.SetDirty(renderer);

		return true;
	}


}