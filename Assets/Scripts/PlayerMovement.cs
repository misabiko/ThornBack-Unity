using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
	Player player;
	PlayerData data;
	CameraController cam;
	new Rigidbody rigidbody;
	Vector3 moveInput;
	Vector3 moveDirection;
	bool sprinting;

	void Awake() {
		player = GetComponent<Player>();
		data = player.data;
		rigidbody = GetComponent<Rigidbody>();
		cam = GetComponentInChildren<CameraController>();
	}

	void Start() {
		var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		var entityQuery = entityManager.CreateEntityQuery(typeof(PlayerChunkCoord));
		var entities = entityQuery.ToEntityArray(Allocator.TempJob);
		
		/*var physicsMass = entityManager.GetComponentData<PhysicsMass>(entities[0]);
		physicsMass.InverseInertia.x = 0;
		physicsMass.InverseInertia.z = 0;
		entityManager.SetComponentData(entities[0], physicsMass);*/

		entities.Dispose();
	}

	void Update() => moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

	void FixedUpdate() {
		if (moveDirection == Vector3.zero) {
			var vel = Flatten(rigidbody.velocity);

			if (vel.magnitude > 0.2f)
				rigidbody.AddForce(-vel.normalized * data.deccel);
			else
				ClampVelocityXZ(0f);
		}else {
			rigidbody.AddForce(moveDirection * data.accel);
			ClampVelocityXZ(sprinting ? data.sprintSpeed : data.speed);
		}
		
		cam.SetSpeed(Flatten(rigidbody.velocity).magnitude);
	}
	
	void ClampVelocityXZ(float maxSpeed) {
		Vector3 flatVel = rigidbody.velocity;
		flatVel.y = 0;

		if (flatVel.magnitude > maxSpeed)
			flatVel = flatVel.normalized * maxSpeed;
		flatVel.y = rigidbody.velocity.y;
		rigidbody.velocity = flatVel;
	}

	static Vector3 Flatten(Vector3 vector) {
		vector.y = 0f;
		return vector;
	}
	
	public void OnMove(InputAction.CallbackContext ctx) {
		Vector2 input = ctx.ReadValue<Vector2>();
		float magnitude = input.magnitude;
		if (magnitude > 1f)
			input.Normalize();

		moveInput = input;
	}

	public void OnSprint(InputAction.CallbackContext ctx) {
		sprinting = ctx.ReadValueAsButton();
		cam.SetSprinting(sprinting);
	}

	public void OnJump(InputAction.CallbackContext ctx) {
		if (!ctx.ReadValueAsButton() || !IsGrounded()) return;
		
		//rigidbody.AddForce(data.jumpForce * Vector3.up, ForceMode.Impulse);
	}

	bool IsGrounded() => true;

	public void OnLook(InputAction.CallbackContext ctx) {
		if (player.cursorCaptured)
			transform.Rotate(Vector3.up, ctx.ReadValue<Vector2>().x * data.camSensitivityX);
	}

	public void OnTweak(InputAction.CallbackContext ctx) {}
}