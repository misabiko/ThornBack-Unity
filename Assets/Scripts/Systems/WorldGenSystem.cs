using Unity.Entities;
using Unity.Mathematics;

public class WorldGenSystem : SystemBase {
	EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;

	protected override void OnCreate() {
		endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		RequireSingletonForUpdate<WorldData>();
	}

	protected override void OnUpdate() {
		EntityCommandBuffer ecb = endSimulationEcbSystem.CreateCommandBuffer();
		
		var worldData = GetSingleton<WorldData>();

		Entities
			.WithAll<ChunkNotGeneratedTag>()
			.ForEach((
				Entity e, int entityInQueryIndex,
				ref DynamicBuffer<WorldBlockData> worldBlockBuffer, in ChunkData chunkData
			) => {
				int chunkVolume = worldData.CHUNK_SIZE * worldData.CHUNK_SIZE * worldData.WORLD_HEIGHT;
				for (int i = 0; i < chunkVolume; i++)
					worldBlockBuffer.Add(new WorldBlockData());

				int chunkWorldX = chunkData.x * worldData.CHUNK_SIZE;
				int chunkWorldY = chunkData.y * worldData.CHUNK_SIZE;

				for (int x = 0; x < worldData.CHUNK_SIZE; x++)
				for (int z = 0; z < worldData.CHUNK_SIZE; z++) {
					float2 n = new float2((x + chunkWorldX) / 5.0f, (z + chunkWorldY) / 5.0f) / 20 - new float2(0.5f, 0.5f);
					
					/*float e1 = Ease((0.5f + noise.pnoise(7.567f * n, new float2(1, 1)) / 2) + 0.02f, -8.57f);

					float e2 = Ease(((0.244f * noise.pnoise(9.588f * n, new float2(1, 1)) * e1) + 1) / 2 + 0.041f, -3.36f) * 2 - 1;

					float e3 = Ease(((0.182f * noise.pnoise(25.246f * n, new float2(1, 1)) * math.max(e1 - 0.433f, 0)) + 1) / 2, -1.57f) * 2 - 1;

					float e0 = e2 + e3;
					
					int y = (int)math.floor(math.clamp((e0 + 3) * 15.3f, 0, worldData.WORLD_HEIGHT - 1));*/
					float elevation = noise.snoise(3f * n);
					
					int y = (int)math.floor(math.clamp(elevation * 10f + 50, 0, worldData.WORLD_HEIGHT - 1));

					//if (x % 2 == 0 && z % 2 == 0) {
					SetBlock(worldBlockBuffer, worldData, x, y, z, 4);

					for (int j = 0; j < y; j++)
						SetBlock(worldBlockBuffer, worldData, x, j, z, 1);
					//}
				}

				ecb.RemoveComponent<ChunkNotGeneratedTag>(e);
			}).Schedule();

		endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
	}

	static float Ease(float s, float curve) {
		if (s < 0)
			s = 0;
		else if (s > 1f)
			s = 1f;
		if (curve > 0) {
			if (curve < 1f)
				return 1f - math.pow(1f - s, 1f / curve);
			else
				return math.pow(s, curve);
		}
		else if (curve < 0) {
			//inout ease

			if (s < 0.5)
				return math.pow(s * 2f, -curve) * 0.5f;
			else
				return (1f - math.pow(1f - (s - 0.5f) * 2f, -curve)) * 0.5f + 0.5f;
		}
		else
			return 0; // no ease (raw)
	}

	static void SetBlock(DynamicBuffer<WorldBlockData> worldBlockBuffer, WorldData worldData, int x, int y, int z, int type)
		=> worldBlockBuffer[x + worldData.CHUNK_SIZE * y + worldData.CHUNK_SIZE * worldData.WORLD_HEIGHT * z] = new WorldBlockData {type = type};
}