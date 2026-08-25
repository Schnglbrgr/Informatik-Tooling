using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;


public class BuildPipelineWindow : EditorWindow {

	private string _buildName = "Your Project Name";
	private string _version = "1.0.0";
	private bool _developmentBuild;

	private SceneAsset _sceneToAdd;
	private Vector2 _sceneScroll;


	[MenuItem("Tools/Build/Open Window")]
	public static void Open() {
		GetWindow<BuildPipelineWindow>("Build Pipeline");
	}


	private void OnEnable() {
		_version = PlayerSettings.bundleVersion;
	}


	private void OnGUI() {
		GUILayout.Label("Build Pipeline", EditorStyles.boldLabel);

		EditorGUILayout.Space();

		_buildName = EditorGUILayout.TextField("Build Name", _buildName);
		_version = EditorGUILayout.TextField("Version", _version);
		_developmentBuild = EditorGUILayout.Toggle("Development Build", _developmentBuild);

		DrawSceneList();

		EditorGUILayout.Space(10);

		if (GUILayout.Button("Build for Windows", GUILayout.Height(35))) {
			BuildWindows();
		}
	}


	private void DrawSceneList() {
		EditorGUILayout.LabelField("Build Scenes", EditorStyles.boldLabel);

		var scenes = EditorBuildSettings.scenes.ToList();

		_sceneScroll = EditorGUILayout.BeginScrollView(_sceneScroll, GUILayout.Height(250));

		for (int i = 0; i < scenes.Count; i++) {
			EditorGUILayout.BeginHorizontal("box");

			var scene = scenes[i];

			scene.enabled = EditorGUILayout.Toggle(scene.enabled, GUILayout.Width(20));

			string sceneName = Path.GetFileNameWithoutExtension(scene.path);

			EditorGUILayout.LabelField($"[{i}] {sceneName}", GUILayout.ExpandWidth(true));

			if (GUILayout.Button("↑", GUILayout.Width(30))) {
				if (i > 0) {
					(scenes[i], scenes[i - 1]) = (scenes[i - 1], scenes[i]);

					SaveScenes(scenes);
					GUIUtility.ExitGUI();
				}
			}

			if (GUILayout.Button("↓", GUILayout.Width(30))) {
				if (i < scenes.Count - 1) {
					(scenes[i], scenes[i + 1]) = (scenes[i + 1], scenes[i]);

					SaveScenes(scenes);
					GUIUtility.ExitGUI();
				}
			}

			if (GUILayout.Button("X", GUILayout.Width(30))) {
				scenes.RemoveAt(i);

				SaveScenes(scenes);
				GUIUtility.ExitGUI();
			}

			scenes[i] = scene;

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();

		SaveScenes(scenes);

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();

		_sceneToAdd = (SceneAsset)EditorGUILayout.ObjectField("Add Scene", _sceneToAdd, typeof(SceneAsset), false);

		if (GUILayout.Button("Add", GUILayout.Width(60))) {
			AddScene();
		}

		EditorGUILayout.EndHorizontal();

		if (_sceneToAdd) {
			AddScene();
		}
	}


	private void AddScene() {
		if (!_sceneToAdd)
			return;

		string path = AssetDatabase.GetAssetPath(_sceneToAdd);

		List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();

		bool alreadyExists = scenes.Any(s => s.path == path);

		if (!alreadyExists) {
			scenes.Add(new EditorBuildSettingsScene(path, true));
			SaveScenes(scenes);
		}

		_sceneToAdd = null;
	}


	private void SaveScenes(List<EditorBuildSettingsScene> scenes) {
		EditorBuildSettings.scenes = scenes.ToArray();
	}


	private void BuildWindows() {
		string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();

		if (scenes.Length == 0) {
			Debug.LogError("No scenes enabled.");
			return;
		}

		PlayerSettings.bundleVersion = _version;

		string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");

		string buildFolder = $"Builds/{_buildName}_{_version}_{timestamp}";

		if (!Directory.Exists(buildFolder)) {
			Directory.CreateDirectory(buildFolder);
		}

		string buildPath = Path.Combine(buildFolder, _buildName + ".exe");

		BuildOptions options = _developmentBuild ? BuildOptions.Development : BuildOptions.None;

		BuildPlayerOptions buildOptions = new BuildPlayerOptions { scenes = scenes, locationPathName = buildPath, target = BuildTarget.StandaloneWindows64, options = options };

		BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

		Debug.Log("Result: " + report.summary.result);
		Debug.Log("Size: " + report.summary.totalSize);
		Debug.Log("Errors: " + report.summary.totalErrors);
		Debug.Log("Warnings: " + report.summary.totalWarnings);

		if (report.summary.result == BuildResult.Succeeded) {
			_version = IncrementVersion(_version);

			PlayerSettings.bundleVersion = _version;

			AssetDatabase.SaveAssets();

			Debug.Log($"Version increased to {_version}");
		}

		Debug.Log($"Build finished at {buildPath}");
	}


	private string IncrementVersion(string currentVersion) {
		string[] parts = currentVersion.Split('.');

		if (parts.Length != 3) {
			Debug.LogWarning("Version format should be Major.Minor.Patch");
			return currentVersion;
		}

		if (!int.TryParse(parts[0], out int major))
			return currentVersion;

		if (!int.TryParse(parts[1], out int minor))
			return currentVersion;

		if (!int.TryParse(parts[2], out int patch))
			return currentVersion;

		patch++;

		return $"{major}.{minor}.{patch}";
	}

}