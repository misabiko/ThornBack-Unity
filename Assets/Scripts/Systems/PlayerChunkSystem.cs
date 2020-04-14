using Unity.Entities;

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
		/*var playerComponent = GetComponent<PlayerComponent>(player);
		var pos = playerTransform.value.position;
		int chunkX = math.floor(pos.x / worldData.CHUNK_SIZE);
		int chunkY = math.floor(pos.z / worldData.CHUNK_SIZE);

		if (chunkX == lastChunkX && chunkY == lastChunkY) return;

		lastChunkX = chunkX;
		lastChunkY = chunkY;*/
	}
}