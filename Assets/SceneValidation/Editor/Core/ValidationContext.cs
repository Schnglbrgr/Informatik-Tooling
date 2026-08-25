using UnityEngine.SceneManagement;


public sealed class ValidationContext {

	public Scene Scene { get; }

	public ValidationProfile Profile { get; }


	public ValidationContext(Scene scene, ValidationProfile profile) {
		Scene = scene;
		Profile = profile;
	}

}