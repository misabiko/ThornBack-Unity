using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
	PlayerData data;
	float yRot;

	void Awake() => data = GetComponentInParent<Player>()?.data;
	
	public void OnLook(InputAction.CallbackContext ctx)
		//=> transform.Rotate(Vector3.right, -ctx.ReadValue<Vector2>().y * data.camSensitivityY);
		=> yRot += ctx.ReadValue<Vector2>().y * data.camSensitivityY;

	void LateUpdate() {
		Debug.Log(yRot);
		yRot = Mathf.Clamp(yRot, -90f, 90f);
		transform.localRotation = Quaternion.Euler(-yRot, 0f, 0f);
		Debug.Log(transform.localRotation);
	}
}