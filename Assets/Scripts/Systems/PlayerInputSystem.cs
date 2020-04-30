using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : SystemBase {
	public InputActionMap actionMap;
	public PlayerData playerData;

	float2 moveInput;
	bool jumpInput;
	float2 lookInput;
	bool sprintInput;
	bool pauseInput;

	const float HALF_PI = math.PI / 2;

	protected override void OnStartRunning() {
		actionMap.Enable();

		actionMap["Move"].started += OnMove;
		actionMap["Move"].performed += OnMove;
		actionMap["Move"].canceled += OnMove;

		actionMap["Look"].started += OnLook;
		actionMap["Look"].performed += OnLook;
		actionMap["Look"].canceled += OnLook;

		actionMap["Sprint"].started += OnSprint;
		actionMap["Sprint"].performed += OnSprint;
		actionMap["Sprint"].canceled += OnSprint;
	}

	void OnSprint(InputAction.CallbackContext ctx) => sprintInput = ctx.ReadValueAsButton();

	void OnLook(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();

	void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();

	void OnPause(InputAction.CallbackContext ctx) => pauseInput = ctx.ReadValueAsButton();

	protected override void OnUpdate() {
		float2 moveInput = this.moveInput;
		bool jumpInput = actionMap["Jump"].triggered;
		float2 lookInput = this.lookInput;
		bool sprintInput = this.sprintInput;
		bool pauseInput = this.pauseInput;

		float camSensitivityY = playerData.camSensitivityY;
		float camSensitivityX = playerData.camSensitivityX;

		Job.WithCode(() => {
			if (math.lengthsq(moveInput) > 1)
				moveInput = math.normalizesafe(moveInput);
		}).Run();

		Entities.ForEach(
			(ref PlayerMoveData moveData, ref PlayerInputData inputData, in LocalToWorld transform)
				=> {
				moveData.moveDirection = transform.Forward * moveInput.y + transform.Right * moveInput.x;
				moveData.jumpInput = jumpInput;
				moveData.yAngle = lookInput.x * camSensitivityX;
				moveData.sprinting = sprintInput;

				inputData.pause = pauseInput;
			}).ScheduleParallel();

		if (!Cursor.visible)
			Entities.ForEach(
				(ref CameraRotationData rotation) => rotation.xAngle = math.clamp(rotation.xAngle + lookInput.y * camSensitivityY, -HALF_PI, HALF_PI)
			).ScheduleParallel();
	}
}