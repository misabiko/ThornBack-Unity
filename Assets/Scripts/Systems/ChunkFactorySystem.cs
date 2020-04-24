using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(PlayerChunkSystem))]
public class ChunkFactorySystem : SystemBase {
	Entity chunkLoaderEntity;
	EntityArchetype chunkArchetype;

	protected override void OnCreate() {
		RequireSingletonForUpdate<ChunkLoaderQueueElement>();

		chunkLoaderEntity = EntityManager.CreateEntity();
		EntityManager.AddBuffer<ChunkLoaderQueueElement>(chunkLoaderEntity);
		EntityManager.SetName(chunkLoaderEntity, "ChunkLoader");

		chunkArchetype = EntityManager.CreateArchetype(
			typeof(ChunkData),
			typeof(Translation),
			typeof(RenderMesh),
			typeof(RenderBounds),
			typeof(LocalToWorld),
			typeof(ChunkSubMeshData),
			typeof(VertexBufferElement),
			typeof(NormalBufferElement),
			typeof(UVBufferElement),
			typeof(IndexBufferElement),
			typeof(WorldBlockData),
			typeof(ChunkMeshingDataElement),
			typeof(PhysicsCollider),
			typeof(ChunkDirtyTag),
			typeof(ChunkNotGeneratedTag)
		);
	}

	protected override void OnUpdate() {
		var blockLibrary = GetSingletonEntity<BlockLibraryData>();
		var blockMaterial = EntityManager.GetBuffer<BlockMaterialElement>(blockLibrary)[0].blockMaterial;
		var opaqueMaterial = EntityManager.GetSharedComponentData<BlockMaterial>(blockMaterial);
		
		var loadingBuffer = EntityManager.GetBuffer<ChunkLoaderQueueElement>(chunkLoaderEntity);
		var loadingBufferCopy = new NativeArray<int2>(loadingBuffer.Length, Allocator.Temp);
		loadingBuffer.Reinterpret<int2>().AsNativeArray().CopyTo(loadingBufferCopy);
		loadingBuffer.Clear();

		foreach (int2 chunk in loadingBufferCopy) {
			var entity = EntityManager.CreateEntity(chunkArchetype);
			EntityManager.SetName(entity, $"Chunk {chunk.x}, {chunk.y}");

			EntityManager.SetComponentData(entity, new ChunkData {
				x = chunk.x, y = chunk.y
			});
			EntityManager.SetComponentData(entity, new Translation {
				Value = new float3(chunk.x * WorldData.CHUNK_SIZE, 0, chunk.y * WorldData.CHUNK_SIZE)
			});
			EntityManager.SetSharedComponentData(entity, new RenderMesh {
				mesh = new Mesh(),
				material = opaqueMaterial
			});
		}

		loadingBufferCopy.Dispose();
	}
}