using UnityEditor;
using UnityEngine;


[ValidationRule]
public class MissingReferenceRule : ValidationRule {

	public override string Id => "missing_reference";
	public override string Name => "Missing Reference";
	public override string Description => "Checks the scene for serialized fields containing missing object references.";

	public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;
	public override ValidationCategory Category => ValidationCategory.References;


	public override void Validate(ValidationContext context, ValidationResultCollection results) {
		GameObject[] rootObjects = context.Scene.GetRootGameObjects();

		foreach (GameObject obj in rootObjects) {
			ValidateGameObject(obj, results);
		}

	}


	private void ValidateGameObject(GameObject gameObject, ValidationResultCollection results) {
		Component[] components = gameObject.GetComponents<Component>();

		foreach (Component component in components) {
			if (!IsUserScript(component))
				continue;

			ValidateComponent(component, gameObject, results);
		}

		foreach (Transform child in gameObject.transform) {
			ValidateGameObject(child.gameObject, results);
		}
	}


	private void ValidateComponent(Component component, GameObject gameObject, ValidationResultCollection results) {
		SerializedObject serializedObject = new SerializedObject(component);
		SerializedProperty property = serializedObject.GetIterator();

		bool enterChildren = true;

		while (property.NextVisible(enterChildren)) {
			enterChildren = false;

			if (property.propertyType != SerializedPropertyType.ObjectReference)
				continue;

			if (property.objectReferenceValue)
				continue;

			if (property.objectReferenceEntityIdValue != EntityId.None)
				continue;

			results.AddResult(Severity, Id, Name, "Missing reference detected.",
				$"Component <i><b>{component.GetType().Name}</b></i> on GameObject <i><b>{gameObject.name}</b></i> contains a missing reference in property <i><b>{property.displayName}</b></i>.",
				gameObject);
		}
	}


	private bool IsUserScript(Component component) {
		if (component is not MonoBehaviour)
			return false;

		return component.GetType().Assembly.GetName().Name == "Assembly-CSharp";
	}

}