using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public class PlayerMovementSystem : SystemBase {
	
	
	protected override void OnUpdate() =>
		Entities
			.WithAll<PlayerComponent>()
			.ForEach(
				(ref PhysicsMass mass) => mass.InverseInertia = float3.zero
			).ScheduleParallel();
}