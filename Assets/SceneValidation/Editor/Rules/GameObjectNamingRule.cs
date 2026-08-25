using System.Text.RegularExpressions;
using UnityEngine;


[ValidationRule]
public sealed class GameObjectNamingRule : ValidationRule {

	public override string Id => "gameObject_naming";
	public override string Name => "GameObject Naming";
	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Info;
	public override string Description => "Checks GameObjects for invalid or suspicious names.";
	public override ValidationCategory Category => ValidationCategory.GameObjects;

	private readonly Regex _cloneNameRegex = new(@"\(\d+\)$");


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		foreach (var root in context.Scene.GetRootGameObjects()) {
			ValidateGameObject(root, results);
		}
	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		if (_cloneNameRegex.IsMatch(gameObject.name)) {
			results.AddResult(Severity, Id, Name, $"GameObject <i><b>'{gameObject.name}'</i></b> appears to be a clone/copy.", "Consider renaming or reviewing this object.", gameObject);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}

}