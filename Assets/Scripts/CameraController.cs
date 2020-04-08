using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
	Player player;
	PlayerData data;
	Animator animator;
	float yRot;
	
	static readonly int Sprinting = Animator.StringToHash("sprinting");

	void Awake() {
		player = GetComponentInParent<Player>();
		data = player.data;
		animator = GetComponent<Animator>();
	}

	public void OnLook(InputAction.CallbackContext ctx) {
		if (player.cursorCaptured)
			yRot += ctx.ReadValue<Vector2>().y * data.camSensitivityY;
	}

	void LateUpdate() {
		yRot = Mathf.Clamp(yRot, -90f, 90f);
		transform.localRotation = Quaternion.Euler(-yRot, 0f, transform.localEulerAngles.z);
	}

	public void SetSprinting(bool sprinting) => animator.SetBool(Sprinting, sprinting);

	public void SetSpeed(float speed) => animator.SetFloat("speed", speed);
}