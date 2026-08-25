using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


[ValidationRule]
public sealed class GameObjectNamingRule : ValidationRule {

	public override string Id => "gameObject_naming";
	public override string Name => "GameObject Naming";
	public override string Description => "Checks GameObjects for invalid or suspicious names.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Info;
	public override ValidationCategory Category => ValidationCategory.GameObjects;

	public override bool CanAutoFix => true;

	private readonly Regex _cloneNameRegex = new(@"^(.*) \((\d+)\)$");


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		foreach (var root in context.Scene.GetRootGameObjects()) {
			ValidateGameObject(root, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		if (_cloneNameRegex.IsMatch(gameObject.name)) {
			results.AddResult(Severity, Id, Name, $"GameObject <i><b>'{gameObject.name}'</i></b> appears to be a clone/copy.", "Consider renaming or reviewing this object.",
				gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}


	public override bool TryAutoFix(ValidationContext context, ValidationResult result) {
		if (result.Target is not GameObject gameObject)
			return false;

		string newName = GetFixedName(gameObject.name);

		if (newName == gameObject.name)
			return false;

		Undo.RecordObject(gameObject, $"Fix {Name}");

		gameObject.name = newName;

		EditorUtility.SetDirty(gameObject);

		return true;
	}


	private string GetFixedName(string name) {
		var match = _cloneNameRegex.Match(name);

		if (!match.Success)
			return name;

		string baseName = match.Groups[1].Value.Trim();
		string numberString = match.Groups[2].Value;

		if (!int.TryParse(numberString, out int number))
			return $"{baseName}_{numberString}";

		return $"{baseName}_{number:00}";
	}

}