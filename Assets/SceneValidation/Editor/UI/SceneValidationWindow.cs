using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class SceneValidationWindow : EditorWindow {

	private const string LastProfileKey = "SceneValidation.LastProfile";

	private ObjectField _profileField;
	private ValidationProfile _selectedProfile;

	private EnumField _severityFilter;
	private EnumField _categoryFilter;

	private Label _sceneLabel;

	private Label _errorCount;
	private Label _warningCount;
	private Label _infoCount;

	private ListView _resultList;
	private VisualElement _detailsPanel;

	private ValidationResultCollection _results;

	private readonly List<ValidationResult> _displayResults = new();


	private enum ValidationSeverityFilter {

		All,
		Error,
		Warning,
		Info

	}


	private enum ValidationCategoryFilter {

		All,
		General,
		References,
		GameObjects,
		Prefabs,
		Rendering,
		Physics,
		Lighting,
		Audio,
		Animation,
		Navigation,
		Performance

	}


	[MenuItem("Tools/Scene Validation/Validation Window &v")]
	public static void OpenWindow() {
		var window = GetWindow<SceneValidationWindow>();

		window.titleContent = new GUIContent("Scene Validation");

		window.minSize = new Vector2(900, 650);
	}


	private void OnEnable() {
		EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
		Undo.undoRedoPerformed += OnUndoRedoPerformed;
	}


	private void OnDisable() {
		EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
		Undo.undoRedoPerformed -= OnUndoRedoPerformed;
	}


	private void OnActiveSceneChanged(Scene previousScene, Scene newScene) {
		RefreshHeader();
		ClearResults();
	}


	private void OnUndoRedoPerformed() {
		var scene = SceneManager.GetActiveScene();

		if (!scene.IsValid() || !_selectedProfile || _results == null)
			return;

		ValidateScene();
	}


	private ValidationProfile GetInitialProfile() {
		ValidationProfile profile = GetLastUsedProfile();

		if (profile)
			return profile;

		profile = FindDefaultProfile();

		if (profile)
			return profile;

		return CreateDefaultProfile();
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


	private void BuildToolbar() {
		var toolbar = new Toolbar();

		var validateButton = new Button(ValidateScene) { text = "Validate Scene" };
		validateButton.AddToClassList("validate-button");

		var clearButton = new Button(ClearResults) { text = "Clear" };
		clearButton.AddToClassList("clear-button");

		toolbar.Add(validateButton);
		toolbar.Add(clearButton);

		var spacer = new ToolbarSpacer { style = { flexGrow = 1 } };
		toolbar.Add(spacer);

		var saveButton = new Button(SaveScene) { text = "Save Scene" };
		saveButton.AddToClassList("save-button");

		toolbar.Add(saveButton);

		rootVisualElement.Add(toolbar);
	}


	private void BuildHeader() {
		var header = new VisualElement();
		header.AddToClassList("validation-header-container");

		_sceneLabel = new Label();
		_sceneLabel.AddToClassList("universal-title-big");

		var profileContainer = new VisualElement();
		profileContainer.AddToClassList("validation-profile-container");

		var profileLabel = new Label("Profile");

		_profileField = new ObjectField { objectType = typeof(ValidationProfile), allowSceneObjects = false };
		_profileField.RegisterValueChangedCallback(evt => {
			_selectedProfile = evt.newValue as ValidationProfile;
			SaveLastUsedProfile(_selectedProfile);
			ClearResults();
			RefreshHeader();
		});

		profileContainer.Add(profileLabel);
		profileContainer.Add(_profileField);

		header.Add(_sceneLabel);
		header.Add(profileContainer);

		rootVisualElement.Add(header);

		_selectedProfile = GetInitialProfile();

		_profileField.SetValueWithoutNotify(_selectedProfile);

		if (_selectedProfile)
			SaveLastUsedProfile(_selectedProfile);

		RefreshHeader();
	}


	private void RefreshHeader() {
		var scene = SceneManager.GetActiveScene();

		_sceneLabel.text = scene.IsValid() ? $"Scene: {scene.name}" : "Scene: No Scene";

		_profileField.SetValueWithoutNotify(_selectedProfile);
	}


	private void BuildSummary() {
		var summary = new VisualElement();
		summary.AddToClassList("validation-summary-container");

		var errorContainer = CreateSummaryItem("Errors:", out _errorCount);
		errorContainer.AddToClassList("summary-error");

		var warningContainer = CreateSummaryItem("Warnings:", out _warningCount);
		warningContainer.AddToClassList("summary-warning");

		var infoContainer = CreateSummaryItem("Infos:", out _infoCount);
		infoContainer.AddToClassList("summary-info");

		summary.Add(errorContainer);
		summary.Add(warningContainer);
		summary.Add(infoContainer);

		rootVisualElement.Add(summary);
	}


	private VisualElement CreateSummaryItem(string label, out Label countLabel) {
		var container = new VisualElement();
		container.AddToClassList("validation-summary-item-container");

		var titleLabel = new Label(label);
		titleLabel.AddToClassList("universal-title-big");

		countLabel = new Label("0");
		countLabel.AddToClassList("validation-summary-count");

		container.Add(titleLabel);
		container.Add(countLabel);

		return container;
	}


	private void BuildResultArea() {
		var container = new TwoPaneSplitView(0, 350f, TwoPaneSplitViewOrientation.Horizontal);
		container.AddToClassList("validation-results-container");

		var resultPanel = new VisualElement();
		resultPanel.AddToClassList("validation-result-panel");

		_detailsPanel = new VisualElement();
		_detailsPanel.AddToClassList("validation-details-panel");

		container.Add(resultPanel);
		container.Add(_detailsPanel);

		var filterContainer = new VisualElement();
		filterContainer.AddToClassList("validation-filter-container");

		_severityFilter = new EnumField("Severity", ValidationSeverityFilter.All);
		_severityFilter.AddToClassList("validation-filter-field");
		_severityFilter.AddToClassList("filter-option-field");

		_categoryFilter = new EnumField("Category", ValidationCategoryFilter.All);
		_categoryFilter.AddToClassList("validation-filter-field");
		_categoryFilter.AddToClassList("filter-option-field");

		_severityFilter.RegisterValueChangedCallback(_ => RefreshResults());
		_categoryFilter.RegisterValueChangedCallback(_ => RefreshResults());

		filterContainer.Add(_severityFilter);
		filterContainer.Add(_categoryFilter);

		resultPanel.Add(filterContainer);

		_resultList = new ListView() { fixedItemHeight = 64, virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight, selectionType = SelectionType.Single };
		_resultList.selectionChanged += OnResultSelected;

		resultPanel.Add(_resultList);

		rootVisualElement.Add(container);

		ShowEmptyDetails();
	}


	private void ValidateScene() {
		var scene = SceneManager.GetActiveScene();

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
			ShowSuccessState();
		}
		else {
			_resultList.style.display = DisplayStyle.Flex;
			ShowEmptyDetails();
		}

		RefreshResults();
	}


	private void ClearResults() {
		_results = null;

		_displayResults.Clear();

		_resultList.itemsSource = null;
		_resultList.style.display = DisplayStyle.Flex;
		_resultList.Rebuild();

		_errorCount.text = "0";
		_warningCount.text = "0";
		_infoCount.text = "0";

		ShowClearedState();
	}


	private void SaveScene() {
		var scene = SceneManager.GetActiveScene();

		if (!scene.IsValid()) {
			Debug.LogWarning("No valid scene is currently open.");
			return;
		}

		if (!scene.isDirty) {
			Debug.Log("Scene has no unsaved changes.");
			return;
		}

		EditorSceneManager.SaveScene(scene);
	}


	private void ShowClearedState() {
		_detailsPanel.Clear();

		var titleLabel = new Label("Validation cleared");
		titleLabel.AddToClassList("universal-title-big");

		var description = new Label("Press 'Validate Scene' to validate the scene again.");
		description.AddToClassList("universal-text");

		_detailsPanel.Add(titleLabel);
		_detailsPanel.Add(description);
	}


	private void RefreshResults() {
		if (_results == null)
			return;

		_displayResults.Clear();

		foreach (var result in _results.Results) {
			if (!MatchesFilters(result))
				continue;

			_displayResults.Add(result);
		}

		_displayResults.Sort((a, b) => GetSeverityOrder(a.Severity).CompareTo(GetSeverityOrder(b.Severity)));

		_resultList.itemsSource = _displayResults;

		_resultList.makeItem = () => new ValidationResultElement();

		_resultList.bindItem = (element, index) => {
			var result = _displayResults[index];
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


	private int GetSeverityOrder(ValidationSeverity severity) {
		return severity switch {
			ValidationSeverity.Error => 0,
			ValidationSeverity.Warning => 1,
			ValidationSeverity.Info => 2,
			_ => 99
		};
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
		titleLabel.AddToClassList("universal-title-big");
		_detailsPanel.Add(titleLabel);


		var severityLabel = new Label(result.Severity.ToString().ToUpper());
		severityLabel.AddToClassList("universal-title-small");
		severityLabel.AddToClassList(GetSeverityTextClass(result.Severity));

		_detailsPanel.Add(severityLabel);

		AddSection("Problem", result.Message);

		var rule = ValidationRuleRegistry.CreateRule(result.RuleId, result.Severity);

		if (rule != null && !string.IsNullOrWhiteSpace(rule.Description)) {
			AddSection("About this rule", rule.Description);
		}

		if (!string.IsNullOrWhiteSpace(result.Details)) {
			AddSection("Details", result.Details);
		}

		if (result.Target) {
			var targetField = new ObjectField("Target") { objectType = typeof(Object), value = result.Target };
			targetField.AddToClassList("universal-title-small");
			targetField.SetEnabled(false);

			_detailsPanel.Add(targetField);

			var buttonRow = new VisualElement();
			buttonRow.AddToClassList("rule-button-row-container");

			var selectButton = new Button(() => { Selection.activeObject = result.Target; }) { text = "Select" };
			selectButton.AddToClassList("select-button");
			var pingButton = new Button(() => { EditorGUIUtility.PingObject(result.Target); }) { text = "Ping" };
			pingButton.AddToClassList("ping-button");

			buttonRow.Add(selectButton);
			buttonRow.Add(pingButton);

			_detailsPanel.Add(buttonRow);

			if (rule is { CanAutoFix: true }) {
				var spacer = new ToolbarSpacer { style = { flexGrow = 1 } };
				buttonRow.Add(spacer);

				var fixButton = new Button(() => AutoFixResult(result)) { text = "Auto Fix" };
				fixButton.AddToClassList("auto-fix-button");

				buttonRow.Add(fixButton);

				var autoFixDetailsContainer = new VisualElement { style = { display = DisplayStyle.None } };
				autoFixDetailsContainer.AddToClassList("autofix-container");

				var autoFixDetails = new Label($"Auto Fix: {rule.AutoFixDetails}");
				autoFixDetails.AddToClassList("universal-text");
				autoFixDetailsContainer.Add(autoFixDetails);

				fixButton.RegisterCallback<MouseEnterEvent>(_ => { autoFixDetailsContainer.style.display = DisplayStyle.Flex; });
				fixButton.RegisterCallback<MouseLeaveEvent>(_ => { autoFixDetailsContainer.style.display = DisplayStyle.None; });

				_detailsPanel.Add(autoFixDetailsContainer);
			}
		}
	}


	private void AddSection(string tl, string content) {
		if (string.IsNullOrWhiteSpace(content))
			return;

		var section = new VisualElement();
		section.AddToClassList("validation-detail-section-container");

		var titleLabel = new Label(tl);
		titleLabel.AddToClassList("universal-title-small");

		var contentLabel = new Label(content);
		contentLabel.AddToClassList("universal-text");

		section.Add(titleLabel);
		section.Add(contentLabel);

		_detailsPanel.Add(section);
	}


	private void AutoFixResult(ValidationResult result) {
		if (!_selectedProfile)
			return;

		var scene = SceneManager.GetActiveScene();

		if (!scene.IsValid())
			return;

		var context = new ValidationContext(scene, _selectedProfile);

		var runner = new ValidationRunner();

		bool success = runner.TryAutoFix(context, result);

		if (!success) {
			Debug.LogWarning($"Could not auto-fix '{result.RuleId}'.");
			return;
		}

		Debug.Log($"Successfully fixed '{result.RuleId}'.");

		ValidateScene();
	}


	private void UpdateSummary() {
		var errors = 0;
		var warnings = 0;
		var infos = 0;

		foreach (var result in _displayResults) {
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

		var titleLabel = new Label("No issue selected");
		titleLabel.AddToClassList("universal-title-big");

		var description = new Label("Select a validation result to view its details.");
		description.AddToClassList("universal-text");

		_detailsPanel.Add(titleLabel);
		_detailsPanel.Add(description);
	}


	private void ShowSuccessState() {
		_resultList.style.display = DisplayStyle.None;

		_detailsPanel.Clear();

		var label = new Label("✓ Scene is valid!");
		label.AddToClassList("universal-title-big");

		_detailsPanel.Add(label);
	}


	private string GetSeverityTextClass(ValidationSeverity severity) {
		return severity switch {
			ValidationSeverity.Error => "severity-error-text",
			ValidationSeverity.Warning => "severity-warning-text",
			ValidationSeverity.Info => "severity-info-text",
			_ => string.Empty
		};
	}


	private ValidationProfile FindDefaultProfile() {
		const string path = "Assets/SceneValidation/ValidationProfiles/DefaultValidationProfile.asset";

		return AssetDatabase.LoadAssetAtPath<ValidationProfile>(path);
	}


	private ValidationProfile GetLastUsedProfile() {
		if (!EditorPrefs.HasKey(LastProfileKey))
			return null;

		string guid = EditorPrefs.GetString(LastProfileKey);

		if (string.IsNullOrEmpty(guid))
			return null;

		string path = AssetDatabase.GUIDToAssetPath(guid);

		if (string.IsNullOrEmpty(path))
			return null;

		return AssetDatabase.LoadAssetAtPath<ValidationProfile>(path);
	}


	private ValidationProfile CreateDefaultProfile() {
		const string folderPath = "Assets/SceneValidation/ValidationProfiles";
		const string assetPath = folderPath + "/DefaultValidationProfile.asset";

		if (!EnsureFolderExists(folderPath))
			return null;

		ValidationProfile profile = AssetDatabase.LoadAssetAtPath<ValidationProfile>(assetPath);

		if (profile)
			return profile;

		profile = CreateInstance<ValidationProfile>();

		profile.ResetToDefaultRules();

		AssetDatabase.CreateAsset(profile, assetPath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log($"Created default Scene Validation Profile at {assetPath}");

		return profile;
	}


	private bool EnsureFolderExists(string folderPath) {
		if (AssetDatabase.IsValidFolder(folderPath))
			return true;

		string[] parts = folderPath.Split('/');

		if (parts.Length == 0)
			return false;

		string currentPath = parts[0];

		for (int i = 1; i < parts.Length; i++) {
			string nextPath = currentPath + "/" + parts[i];

			if (!AssetDatabase.IsValidFolder(nextPath)) {
				string guid = AssetDatabase.CreateFolder(currentPath, parts[i]);

				if (string.IsNullOrEmpty(guid)) {
					Debug.LogError($"Could not create folder '{nextPath}'.");
					return false;
				}
			}

			currentPath = nextPath;
		}

		return AssetDatabase.IsValidFolder(folderPath);
	}


	private void SaveLastUsedProfile(ValidationProfile profile) {
		if (!profile) {
			EditorPrefs.DeleteKey(LastProfileKey);
			return;
		}

		string path = AssetDatabase.GetAssetPath(profile);

		if (string.IsNullOrEmpty(path)) {
			EditorPrefs.DeleteKey(LastProfileKey);
			return;
		}

		string guid = AssetDatabase.AssetPathToGUID(path);

		if (string.IsNullOrEmpty(guid)) {
			EditorPrefs.DeleteKey(LastProfileKey);
			return;
		}

		EditorPrefs.SetString(LastProfileKey, guid);
	}


	private bool MatchesFilters(ValidationResult result) {
		if (!MatchesSeverityFilter(result))
			return false;

		if (!MatchesCategoryFilter(result))
			return false;

		return true;
	}


	private bool MatchesSeverityFilter(ValidationResult result) {
		var filter = (ValidationSeverityFilter)_severityFilter.value;

		return filter switch {
			ValidationSeverityFilter.All => true,
			ValidationSeverityFilter.Error => result.Severity == ValidationSeverity.Error,
			ValidationSeverityFilter.Warning => result.Severity == ValidationSeverity.Warning,
			ValidationSeverityFilter.Info => result.Severity == ValidationSeverity.Info,
			_ => true
		};
	}


	private bool MatchesCategoryFilter(ValidationResult result) {
		var filter = (ValidationCategoryFilter)_categoryFilter.value;

		if (filter == ValidationCategoryFilter.All)
			return true;

		var rule = ValidationRuleRegistry.CreateRule(result.RuleId, result.Severity);

		if (rule == null)
			return false;

		return rule.Category.ToString() == filter.ToString();
	}

}