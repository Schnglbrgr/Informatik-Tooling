using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[ValidationRule]
public class DuplicateGameObjectNameRule : ValidationRule {

	public override string Id => "gameObject-duplicate_name";
	public override string Name => "Duplicate GameObject Name";
	public override string Description => "Checks for GameObjects with duplicate names.";
	public override string AutoFixDetails => "Renames the GameObject to ensure a unique name by appending a numeric suffix such as '_1', '_2', etc.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Info;
	public override ValidationCategory Category => ValidationCategory.GameObjects;

	public override bool CanAutoFix => true;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		foreach (var root in context.Scene.GetRootGameObjects()) {
			ValidateChildren(root.transform, results);
		}
	}


	private void ValidateChildren(Transform parent, ValidationResultCollection results) {
		var childrenByName = new Dictionary<string, List<GameObject>>();

		foreach (Transform child in parent) {
			if (!childrenByName.TryGetValue(child.name, out var list)) {
				list = new List<GameObject>();
				childrenByName.Add(child.name, list);
			}

			list.Add(child.gameObject);
		}

		foreach (var pair in childrenByName) {
			if (pair.Value.Count <= 1)
				continue;

			foreach (var gameObject in pair.Value) {
				results.AddResult(Severity, Id, Name, $"GameObject <i><b>{gameObject.name}</b></i> has a duplicate sibling name.",
					"Consider giving sibling GameObjects unique names.", gameObject);
			}
		}

		foreach (Transform child in parent) {
			ValidateChildren(child, results);
		}
	}


	public override bool TryAutoFix(ValidationContext context, ValidationResult result) {
		if (result.Target is not GameObject gameObject)
			return false;

		Transform parent = gameObject.transform.parent;

		if (!parent)
			return false;

		var usedNames = new HashSet<string>();

		foreach (Transform sibling in parent) {
			if (sibling.gameObject == gameObject)
				continue;

			usedNames.Add(sibling.name);
		}

		string originalName = gameObject.name;

		if (!usedNames.Contains(originalName))
			return false;

		int index = 1;
		string newName;

		do {
			newName = $"{originalName}_{index:00}";
			index++;
		} while (usedNames.Contains(newName));

		Undo.RecordObject(gameObject, $"Fix {Name}");

		gameObject.name = newName;

		EditorUtility.SetDirty(gameObject);

		return true;
	}

}