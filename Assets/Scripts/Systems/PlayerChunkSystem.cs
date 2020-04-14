using Unity.Entities;
using Unity.Mathematics;

public class PlayerChunkSystem : SystemBase {
	Entity player;
	PlayerTransform playerTransform;
	
	protected override void OnCreate() {
		RequireSingletonForUpdate<PlayerComponent>();
		RequireSingletonForUpdate<WorldData>();
	}

	protected override void OnStartRunning() {
		player = GetSingletonEntity<PlayerComponent>();
		playerTransform = EntityManager.GetSharedComponentData<PlayerTransform>(player);
	}

	protected override void OnUpdate() {
		float3 pos = playerTransform.value.position;
		
		Entities.ForEach((ref PlayerComponent playerComponent) => {
			int chunkX = (int) math.floor(pos.x / WorldData.CHUNK_SIZE);
			int chunkY = (int) math.floor(pos.z / WorldData.CHUNK_SIZE);

			if (chunkX == playerComponent.chunkX && chunkY == playerComponent.chunkY) return;

			playerComponent.chunkX = chunkX;
			playerComponent.chunkY = chunkY;
		}).ScheduleParallel();
	}
}