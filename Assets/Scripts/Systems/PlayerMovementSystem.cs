using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class PlayerMovementSystem : SystemBase {
	public PlayerData playerData;
	
	protected override void OnUpdate() {
		//TODO Make that a one off
		Entities
			.WithAll<PlayerChunkCoord>()
			.ForEach(
				(ref PhysicsMass mass) => mass.InverseInertia = float3.zero
			).ScheduleParallel();

		float deccel = playerData.deccel;
		
		Entities
			.WithAll<PlayerChunkCoord>()
			.ForEach(
				(ref PhysicsVelocity velocity) => {
					float3 flatVel = velocity.Linear;
					flatVel.y = 0;

					if (math.length(flatVel) > 0.2f)
						velocity.Linear -= math.normalize(flatVel) * deccel;
					else {
						velocity.Linear.x = 0;
						velocity.Linear.z = 0;
					}
				}
			).ScheduleParallel();
	}
}