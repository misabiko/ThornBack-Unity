using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : SystemBase {
	public InputActionMap actionMap;

	protected override void OnStartRunning() => actionMap.Enable();

	protected override void OnUpdate() {
		float2 moveInput = actionMap["Move"].ReadValue<Vector2>();
		
		Job.WithCode(() => {
			if (math.lengthsq(moveInput) > 1)
				moveInput = math.normalizesafe(moveInput);
		}).Run();

		Entities.ForEach(
			(ref PlayerMoveData moveData, in LocalToWorld transform)
				=> moveData.moveDirection = transform.Forward * moveInput.y + transform.Right * moveInput.x
		).ScheduleParallel();
	}
}