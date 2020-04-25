using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[UpdateAfter(typeof(PlayerInputSystem))]
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
		float accel = playerData.accel;
		float speed = playerData.speed;
		
		Entities
			.WithAll<PlayerChunkCoord>()
			.ForEach(
				(ref PhysicsVelocity velocity, in PlayerMoveData moveData) => {
					if (moveData.moveDirection.Equals(float3.zero)) {
						float3 flatVel = velocity.Linear;
						flatVel.y = 0;

						if (math.length(flatVel) > 0.2f)
							velocity.Linear -= math.normalize(flatVel) * deccel;
						else
							ClampVelocityXZ(ref velocity, 0);
					}else {
						velocity.Linear += moveData.moveDirection * accel;
						ClampVelocityXZ(ref velocity, speed);
					}
				}
			).ScheduleParallel();
	}

	static void ClampVelocityXZ(ref PhysicsVelocity velocity, float maxSpeed) {
		float3 flatVel = velocity.Linear;
		flatVel.y = 0;

		if (math.length(flatVel) > maxSpeed)
			flatVel = math.normalizesafe(flatVel) * maxSpeed;
		flatVel.y = velocity.Linear.y;
		velocity.Linear = flatVel;
	}
}