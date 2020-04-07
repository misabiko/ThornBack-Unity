using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
	PlayerData data;
	new Rigidbody rigidbody;
	Transform camTransform;
	Vector3 moveInput;
	Vector3 moveDirection;
	bool sprinting;

	void Awake() {
		data = GetComponent<Player>()?.data;
		rigidbody = GetComponent<Rigidbody>();

		Camera cam = Camera.main;
		if (cam != null)
			camTransform = cam.transform;
	}

	void Update() => moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

	void FixedUpdate() {
		rigidbody.AddForce(moveDirection * data.accel);
		ClampVelocityXZ(sprinting ? data.sprintSpeed : data.speed);
	}
	
	void ClampVelocityXZ(float maxSpeed) {
		Vector3 flatVel = rigidbody.velocity;
		flatVel.y = 0;

		if (flatVel.magnitude > maxSpeed)
			flatVel = flatVel.normalized * maxSpeed;
		flatVel.y = rigidbody.velocity.y;
		rigidbody.velocity = flatVel;
	}
	
	public void OnMove(InputAction.CallbackContext ctx) {
		Vector2 input = ctx.ReadValue<Vector2>();
		float magnitude = input.magnitude;
		if (magnitude > 1f)
			input.Normalize();

		moveInput = input;
	}

	public void OnSprint(InputAction.CallbackContext ctx) => sprinting = ctx.ReadValueAsButton();

	public void OnJump(InputAction.CallbackContext ctx) {
		if (!ctx.ReadValueAsButton() || !IsGrounded()) return;
		
		rigidbody.AddForce(data.jumpForce * Vector3.up, ForceMode.Impulse);
	}

	bool IsGrounded() => true;

	public void OnLook(InputAction.CallbackContext ctx) {
		var lookInput = ctx.ReadValue<Vector2>();
		
		transform.Rotate(Vector3.up, lookInput.x * data.camSensitivityX);
		camTransform.Rotate(Vector3.right, -lookInput.y * data.camSensitivityY);
	}
}