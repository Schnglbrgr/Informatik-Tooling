using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class SceneValidationWindow : EditorWindow {

	private ObjectField _profileField;
	private ValidationProfile _selectedProfile;

	private Label _sceneLabel;

	private Label _errorCount;
	private Label _warningCount;
	private Label _infoCount;

	private ListView _resultList;
	private VisualElement _detailsPanel;

	private ValidationResultCollection _results;


	[MenuItem("Tools/Scene Validation/Validation Window &v")]
	public static void OpenWindow() {
		var window = GetWindow<SceneValidationWindow>();

		window.titleContent = new GUIContent("Scene Validation");

		window.minSize = new Vector2(900, 500);
	}


	private void CreateGUI() {
		var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SceneValidation/Editor/UI/SceneValidationWindow.uss");

		if (styleSheet)
			rootVisualElement.styleSheets.Add(styleSheet);

		BuildHeader();
		BuildToolbar();
		BuildSummary();
		BuildResultArea();

		RefreshHeader();
	}


	private ValidationProfile FindDefaultProfile() {
		string[] guids = AssetDatabase.FindAssets("t:ValidationProfile");

		foreach (string guid in guids) {
			string path = AssetDatabase.GUIDToAssetPath(guid);

			ValidationProfile profile = AssetDatabase.LoadAssetAtPath<ValidationProfile>(path);

			if (!profile)
				continue;

			if (profile.name == "DefaultValidationProfile")
				return profile;
		}

		return null;
	}


	private void BuildToolbar() {
		var toolbar = new Toolbar();

		var validateButton = new Button(ValidateScene) { text = "Validate Scene" };
		validateButton.AddToClassList("validate-button");

		toolbar.Add(validateButton);
		rootVisualElement.Add(toolbar);
	}


	private void BuildHeader() {
		var header = new VisualElement();
		header.AddToClassList("validation-header");

		_sceneLabel = new Label();

		var profileContainer = new VisualElement();
		profileContainer.AddToClassList("validation-profile-container");

		var profileLabel = new Label("Profile");

		_profileField = new ObjectField { objectType = typeof(ValidationProfile), allowSceneObjects = false };
		_profileField.RegisterValueChangedCallback(evt => {
			_selectedProfile = evt.newValue as ValidationProfile;
			RefreshHeader();
		});

		profileContainer.Add(profileLabel);
		profileContainer.Add(_profileField);

		header.Add(_sceneLabel);
		header.Add(profileContainer);

		rootVisualElement.Add(header);

		_selectedProfile = FindDefaultProfile();

		_profileField.SetValueWithoutNotify(_selectedProfile);

		RefreshHeader();
	}


	private void RefreshHeader() {
		var scene = EditorSceneManager.GetActiveScene();

		_sceneLabel.text = scene.IsValid() ? $"Scene: {scene.name}" : "Scene: No Scene";

		_profileField.SetValueWithoutNotify(_selectedProfile);
	}


	private void BuildSummary() {
		var summary = new VisualElement();
		summary.AddToClassList("validation-summary");

		var errorContainer = CreateSummaryItem("Errors", out _errorCount);
		errorContainer.AddToClassList("summary-error");

		var warningContainer = CreateSummaryItem("Warnings", out _warningCount);
		warningContainer.AddToClassList("summary-warning");

		var infoContainer = CreateSummaryItem("Info", out _infoCount);
		infoContainer.AddToClassList("summary-info");

		summary.Add(errorContainer);
		summary.Add(warningContainer);
		summary.Add(infoContainer);

		rootVisualElement.Add(summary);
	}


	private VisualElement CreateSummaryItem(string label, out Label countLabel) {
		var container = new VisualElement();
		container.AddToClassList("validation-summary-item");

		var titleLabel = new Label(label);
		titleLabel.AddToClassList("validation-summary-title");

		countLabel = new Label("0");
		countLabel.AddToClassList("validation-summary-count");

		container.Add(titleLabel);
		container.Add(countLabel);

		return container;
	}


	private void BuildResultArea() {
		var container = new TwoPaneSplitView(0, 350, TwoPaneSplitViewOrientation.Horizontal);
		container.AddToClassList("validation-results-container");

		var resultPanel = new VisualElement();
		_detailsPanel = new VisualElement();

		container.Add(resultPanel);
		container.Add(_detailsPanel);

		_resultList = new ListView() { fixedItemHeight = 64, selectionType = SelectionType.Single };
		_resultList.selectionChanged += OnResultSelected;

		resultPanel.Add(_resultList);

		rootVisualElement.Add(container);

		ShowEmptyDetails();
	}


	private void ValidateScene() {
		var scene = EditorSceneManager.GetActiveScene();

		if (!scene.IsValid()) {
			Debug.LogWarning("No valid scene is currently open");
			return;
		}

		if (!_selectedProfile) {
			Debug.LogWarning("No validation profile selected.");
			return;
		}

		var context = new ValidationContext(scene, _selectedProfile);
		var runner = new ValidationRunner();

		_results = runner.Validate(context);

		if (_results.Results.Count == 0) {
			ShowEmptyDetails();
		}

		RefreshResults();
	}


	private void RefreshResults() {
		if (_results == null)
			return;

		var results = _results.Results;

		_resultList.itemsSource = results as IList;

		_resultList.makeItem = () => new ValidationResultElement();

		_resultList.bindItem = (element, index) => {
			var result = results[index];
			var resultElement = (ValidationResultElement)element;

			resultElement.Bind(result);
		};

		UpdateSummary();
		_resultList.Rebuild();
	}


	private void OnResultSelected(IEnumerable<object> selection) {
		foreach (var item in selection) {
			if (item is not ValidationResult result)
				continue;

			ShowDetails(result);
			SelectTarget(result);

			break;
		}
	}


	private void SelectTarget(ValidationResult result) {
		if (!result.Target)
			return;

		Selection.activeObject = result.Target;

		EditorGUIUtility.PingObject(result.Target);
	}


	private void ShowDetails(ValidationResult result) {
		_detailsPanel.Clear();

		var titleLabel = new Label(result.RuleName);
		titleLabel.AddToClassList("validation-detail-title");
		_detailsPanel.Add(titleLabel);


		var severityLabel = new Label(result.Severity.ToString().ToUpper());
		severityLabel.AddToClassList("validation-detail-severity");
		_detailsPanel.Add(severityLabel);

		AddSection("Problem", result.Message);

		var rule = ValidationRuleRegistry.CreateRule(result.RuleId);

		if (rule != null && !string.IsNullOrWhiteSpace(rule.Description)) {
			AddSection("About this rule", rule.Description);
		}

		if (!string.IsNullOrWhiteSpace(result.Details)) {
			AddSection("Details", result.Details);
		}

		if (result.Target) {
			var targetField = new ObjectField("Target") { objectType = typeof(Object), value = result.Target };

			targetField.SetEnabled(false);

			_detailsPanel.Add(targetField);

			var buttonRow = new VisualElement();
			buttonRow.AddToClassList("validation-button-row");

			var selectButton = new Button(() => { Selection.activeObject = result.Target; }) { text = "Select" };
			var pingButton = new Button(() => { EditorGUIUtility.PingObject(result.Target); }) { text = "Ping" };

			buttonRow.Add(selectButton);
			buttonRow.Add(pingButton);

			_detailsPanel.Add(buttonRow);
		}

	}


	private void AddSection(string tl, string content) {
		if (string.IsNullOrWhiteSpace(content))
			return;

		var section = new VisualElement();
		section.AddToClassList("validation-detail-section");

		var titleLabel = new Label(tl);
		titleLabel.AddToClassList("validation-detail-section-title");

		var contentLabel = new Label(content);
		contentLabel.AddToClassList("validation-detail-section-content");

		section.Add(titleLabel);
		section.Add(contentLabel);

		_detailsPanel.Add(section);
	}


	private void FixResult(ValidationResult result) {
		Debug.LogWarning($"Auto-fix is not implemented yet for '{result.RuleId}'.");
	}


	private void UpdateSummary() {
		var errors = 0;
		var warnings = 0;
		var infos = 0;

		foreach (var result in _results.Results) {
			switch (result.Severity) {
				case ValidationSeverity.Error:
					errors++;
					break;

				case ValidationSeverity.Warning:
					warnings++;
					break;

				case ValidationSeverity.Info:
					infos++;
					break;
			}
		}

		_errorCount.text = errors.ToString();
		_warningCount.text = warnings.ToString();
		_infoCount.text = infos.ToString();
	}


	private void ShowEmptyDetails() {
		_detailsPanel.Clear();

		var title = new Label("No issue selected");
		title.AddToClassList("validation-empty-title");

		var description = new Label("Select a validation result to view its details.");
		description.AddToClassList("validation-empty-description");

		_detailsPanel.Add(title);
		_detailsPanel.Add(description);
	}


	private void ShowSuccessState() {
		_resultList.style.display = DisplayStyle.None;

		_detailsPanel.Clear();

		var label = new Label("✓ Scene is valid!");

		_detailsPanel.Add(label);
	}

}