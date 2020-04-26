using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : SystemBase {
	public InputActionMap actionMap;
	public PlayerData playerData;
	
	const float HALF_PI = math.PI / 2;

	protected override void OnStartRunning() => actionMap.Enable();

	protected override void OnUpdate() {
		float2 moveInput = actionMap["Move"].ReadValue<Vector2>();
		bool jumpInput = actionMap["Jump"].triggered;
		float2 lookInput = actionMap["Look"].ReadValue<Vector2>();

		float camSensitivityY = playerData.camSensitivityY;
		float camSensitivityX = playerData.camSensitivityX;
		
		Job.WithCode(() => {
			if (math.lengthsq(moveInput) > 1)
				moveInput = math.normalizesafe(moveInput);
		}).Run();

		Entities.ForEach(
			(ref PlayerMoveData moveData, in LocalToWorld transform)
				=> {
				moveData.moveDirection = transform.Forward * moveInput.y + transform.Right * moveInput.x;
				moveData.jumpInput = jumpInput;
				moveData.yAngle = lookInput.x * camSensitivityX;
			}).ScheduleParallel();

		Entities.ForEach(
			(ref CameraRotationData rotation) => rotation.xAngle = math.clamp(rotation.xAngle + lookInput.y * camSensitivityY, -HALF_PI, HALF_PI)
		).ScheduleParallel();
	}
}