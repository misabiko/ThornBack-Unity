using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
	public PlayerData data;
	public bool cursorCaptured {get; private set;}

	void Awake() => cursorCaptured = true;

	void Start() {
		GetComponent<PlayerInput>().actions["Fire"].performed += (context) => CaptureCursor();

		var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		var playerEntity = entityManager.CreateEntity(typeof(PlayerComponent), typeof(PlayerTransform));
		entityManager.SetName(playerEntity, "Player");
		entityManager.SetSharedComponentData(playerEntity, new PlayerTransform{value = transform});

		var playerQuery = entityManager.CreateEntityQuery(typeof(PlayerComponent));
		playerQuery.SetSingleton(new PlayerComponent {chunkX = 0, chunkY = 0});
	}
	
	void OnApplicationFocus(bool hasFocus) {
		if (hasFocus) 
			CaptureCursor();
		else
			FreeCursor();
	}

	//Temporary, until adding pause menu
	public void OnPause(InputAction.CallbackContext ctx) {
		if (!ctx.ReadValueAsButton()) return;
		
		FreeCursor();
	}

	void FreeCursor() {
		cursorCaptured = false;
		
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	void CaptureCursor() {
		Debug.Log("Capturing Cursor!");
		cursorCaptured = true;
		
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}