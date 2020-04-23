using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerChunkSystem : SystemBase {
	Entity player;
	PlayerTransform playerTransform;
	EntityQuery chunkQuery;

	protected override void OnCreate() {
		RequireSingletonForUpdate<PlayerComponent>();
		RequireSingletonForUpdate<ChunkLoaderQueueElement>();
	}

	protected override void OnStartRunning() {
		player = GetSingletonEntity<PlayerComponent>();
		playerTransform = EntityManager.GetSharedComponentData<PlayerTransform>(player);
	}

	protected override void OnUpdate() {
		float3 pos = playerTransform.value.position;

		var chunks = new NativeArray<int2>(chunkQuery.CalculateEntityCount(), Allocator.TempJob);

		Entities
			.WithStoreEntityQueryInField(ref chunkQuery)
			.ForEach(
				(int entityInQueryIndex, in ChunkData chunkData)
					=> chunks[entityInQueryIndex] = new int2(chunkData.x, chunkData.y)
			).Schedule();

		const int radius = 4;
		var loadingBacklog = new NativeArray<int2>(4 * radius * radius, Allocator.TempJob);
		var loadingBacklogLength = new NativeArray<int>(1, Allocator.TempJob);
		loadingBacklogLength[0] = 0;
		
		Entities.ForEach((Entity e, int entityInQueryIndex, ref PlayerComponent playerComponent) => {
				int chunkX = (int) math.floor(pos.x / WorldData.CHUNK_SIZE);
				int chunkY = (int) math.floor(pos.z / WorldData.CHUNK_SIZE);

				if (chunkX == playerComponent.chunkX && chunkY == playerComponent.chunkY) return;

				playerComponent.chunkX = chunkX;
				playerComponent.chunkY = chunkY;

				int2 coord = new int2();
				for (coord.x = chunkX - radius; coord.x < chunkX + radius; coord.x++)
				for (coord.y = chunkY - radius; coord.y < chunkY + radius; coord.y++) {
					if (
						math.pow(coord.x - chunkX, 2) + math.pow(coord.y - chunkY, 2) <= radius * radius &&
						!chunks.Contains(coord)
					)
						loadingBacklog[loadingBacklogLength[0]++] = coord;
				}
			})
			.WithReadOnly(chunks)
			.WithDeallocateOnJobCompletion(chunks)
			.Schedule();

		Entities.ForEach((ref DynamicBuffer<ChunkLoaderQueueElement> loadingQueue) => {
				for (int index = 0; index < loadingBacklogLength[0]; index++)
					loadingQueue.Add(loadingBacklog[index]);
			})
			.WithReadOnly(loadingBacklog)
			.WithReadOnly(loadingBacklogLength)
			.WithDeallocateOnJobCompletion(loadingBacklog)
			.WithDeallocateOnJobCompletion(loadingBacklogLength)
			.Schedule();
	}
}