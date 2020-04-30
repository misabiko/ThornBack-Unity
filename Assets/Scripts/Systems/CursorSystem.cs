using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorSystem : SystemBase {
	public InputAction pauseInput;
	public InputAction hitInput;
	
	protected override void OnStartRunning() {
		pauseInput.performed += _ => FreeCursor();
		hitInput.performed += _ => CaptureCursor();
		
		CaptureCursor();
	}

	static void FreeCursor() {
		Debug.Log("Freeing Cursor!");
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	static void CaptureCursor() {
		Debug.Log("Capturing Cursor!");
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	protected override void OnUpdate() {}
}