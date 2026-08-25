using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Toolbars;
using UnityEngine;


public static class EditorToolbarSceneButton {

	[MainToolbarElement("Scenes Button", defaultDockPosition = MainToolbarDockPosition.Right)]
	public static MainToolbarElement GetScenesButton() {
		Texture2D assetIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
		return new MainToolbarButton(new MainToolbarContent("Scenes", assetIcon, "Opens the scene selection tool"), OpenSearchableMenu);
	}


	private static void OpenSearchableMenu() {
		SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), ScriptableObject.CreateInstance<SceneSearchableMenu>());
	}

}