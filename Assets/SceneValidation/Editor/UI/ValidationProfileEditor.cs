using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ValidationProfile))]
public class ValidationProfileEditor : Editor {

	private ValidationProfile _profile;


	private void OnEnable() {
		_profile = (ValidationProfile)target;
	}


	public override void OnInspectorGUI() {
		serializedObject.Update();

		DrawHeader();
		DrawRules();
		DrawButtons();

		serializedObject.ApplyModifiedProperties();

		if (GUI.changed)
			EditorUtility.SetDirty(_profile);
	}


	private new void DrawHeader() {
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Validation Profile", EditorStyles.boldLabel);
		EditorGUILayout.LabelField($"Rules: {_profile.RuleConfigurations.Count}");

		EditorGUILayout.Space();
	}


	private void DrawRules() {
		foreach (var config in _profile.RuleConfigurations)
			DrawRule(config);
	}


	private void DrawRule(RuleConfiguration config) {
		var rule = ValidationRuleRegistry.CreateRule(config.ruleId, config.severityOverride);

		EditorGUILayout.BeginVertical("box");

		if (rule == null) {
			EditorGUILayout.LabelField("Unknown Rule", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox($"The rule '{config.ruleId}' could not be found.", MessageType.Error);

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Remove")) {
				RemoveRule(config);
				GUIUtility.ExitGUI();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			return;
		}

		EditorGUILayout.BeginHorizontal();

		config.enabled = EditorGUILayout.Toggle(config.enabled, GUILayout.Width(18));
		EditorGUILayout.LabelField(rule.Name, EditorStyles.whiteBoldLabel);

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Remove", GUILayout.Width(70))) {
			RemoveRule(config);
			GUIUtility.ExitGUI();
		}

		EditorGUILayout.EndHorizontal();

		if (!string.IsNullOrWhiteSpace(rule.Description)) {
			EditorGUILayout.HelpBox(rule.Description, MessageType.None);
		}

		EditorGUILayout.LabelField("Category", rule.Category.ToString());

		config.severityOverride = (ValidationSeverity)EditorGUILayout.EnumPopup("Severity", config.severityOverride);

		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();
	}


	private void DrawButtons() {
		EditorGUILayout.Space();

		if (GUILayout.Button("+ Add New Rule")) {
			ShowAddRuleMenu();
		}

		if (GUILayout.Button("Reset Configurations")) {
			Undo.RecordObject(_profile, "Reset Validation Configurations");
			_profile.ResetConfigurations();
			EditorUtility.SetDirty(_profile);
		}
		
		EditorGUILayout.Space();

		if (GUILayout.Button("Reset To Default")) {
			if (!EditorUtility.DisplayDialog("Reset Validation Profile", "This will remove all current rules and restore the default rules. Continue?", "Reset", "Cancel"))
				return;

			Undo.RecordObject(_profile, "Reset Validation Rules");
			_profile.ResetToDefaultRules();
			EditorUtility.SetDirty(_profile);
		}
	}


	private void ShowAddRuleMenu() {
		var menu = new GenericMenu();

		bool hasAvailableRules = false;

		foreach (var rule in ValidationRuleRegistry.CreateRules()) {
			if (_profile.HasRule(rule.Id))
				continue;

			hasAvailableRules = true;

			var ruleId = rule.Id;

			menu.AddItem(new GUIContent(rule.Name), false, () => AddRule(ruleId));
		}

		if (!hasAvailableRules)
			menu.AddDisabledItem(new GUIContent("No more rules available"));

		menu.ShowAsContext();
	}


	private void AddRule(string ruleId) {
		Undo.RecordObject(_profile, "Add Validation Rule");
		_profile.AddRule(ruleId);
		EditorUtility.SetDirty(_profile);

		Repaint();
	}


	private void RemoveRule(RuleConfiguration config) {
		Undo.RecordObject(_profile, "Remove Validation Rule");
		_profile.RemoveRule(config.ruleId);
		EditorUtility.SetDirty(_profile);
	}

}