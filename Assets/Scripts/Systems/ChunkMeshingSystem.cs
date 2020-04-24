using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public class ChunkMeshingSystem : SystemBase {
	enum Direction {
		North = 0,
		South = 1,
		East = 2,
		West = 3,
		Top = 4,
		Bottom = 5
	}

	EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;

	protected override void OnCreate() {
		endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		RequireSingletonForUpdate<BlockLibraryData>();
	}

	protected override void OnUpdate() {
		//ToConcurrent makes it so we can use it in parallel jobs
		EntityCommandBuffer.Concurrent ecb = endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

		Entity blockLibrary = GetSingletonEntity<BlockLibraryData>();
		var blockTypes = EntityManager.GetBuffer<BlockTypeData>(blockLibrary).ToNativeArray(Allocator.TempJob);

		Entities
			.WithAll<ChunkDirtyTag>()
			.WithNone<ChunkNotGeneratedTag>()
			.ForEach((
				ref DynamicBuffer<ChunkMeshingDataElement> meshingData,
				in DynamicBuffer<WorldBlockData> worldBlockBuffer
			) => {
				var voxelMask = new NativeArray<bool>(WorldData.CHUNK_SIZE * WorldData.WORLD_HEIGHT * WorldData.CHUNK_SIZE, Allocator.Temp);
				int3 pos;
				
				for (pos.x = 0; pos.x < WorldData.CHUNK_SIZE; pos.x++)
				for (pos.y = 0; pos.y < WorldData.WORLD_HEIGHT; pos.y++)
				for (pos.z = 0; pos.z < WorldData.CHUNK_SIZE; pos.z++)
					if (!voxelMask[Get3dIndex(pos)] && worldBlockBuffer[Get3dIndex(pos)].type > 0) {
						voxelMask[Get3dIndex(pos)] = true;
						int3 boxSize = new int3(1);

						for (int3 d = new int3(1, 0, 0); d.x < WorldData.CHUNK_SIZE - pos.x; d.x++) {
							if (voxelMask[Get3dIndex(pos + d)] || worldBlockBuffer[Get3dIndex(pos + d)].type != worldBlockBuffer[Get3dIndex(pos)].type) {
								boxSize.x += d.x - 1;
								goto fullbreak1;
							}

							voxelMask[Get3dIndex(pos + d)] = true;
						}

						boxSize.x += WorldData.CHUNK_SIZE - pos.x - 1; //This is skipped if goto fullbreak1
						fullbreak1:

						for (int3 d = new int3(0, 0, 1); d.z < WorldData.CHUNK_SIZE - pos.z; d.z++) {
							for (d.x = 0; d.x < boxSize.x; d.x++)
								if (voxelMask[Get3dIndex(pos + d)] || worldBlockBuffer[Get3dIndex(pos + d)].type != worldBlockBuffer[Get3dIndex(pos)].type) {
									boxSize.z += d.z - 1;
									goto fullbreak2;
								}

							for (d.x = 0; d.x < boxSize.x; d.x++)
								voxelMask[Get3dIndex(pos + d)] = true;
						}

						boxSize.z += WorldData.CHUNK_SIZE - pos.z - 1; //This is skipped if goto fullbreak2
						fullbreak2:

						for (int3 d = new int3(0, 1, 0); d.y < WorldData.WORLD_HEIGHT - pos.y; d.y++) {
							for (d.z = 0; d.z < boxSize.z; d.z++)
							for (d.x = 0; d.x < boxSize.x; d.x++)
								if (voxelMask[Get3dIndex(pos + d)] || worldBlockBuffer[Get3dIndex(pos + d)].type != worldBlockBuffer[Get3dIndex(pos)].type) {
									boxSize.y += d.y - 1;
									goto fullbreak3;
								}

							for (d.z = 0; d.z < boxSize.z; d.z++)
							for (d.x = 0; d.x < boxSize.x; d.x++)
								voxelMask[Get3dIndex(pos + d)] = true;
						}

						boxSize.y += WorldData.WORLD_HEIGHT - pos.y - 1; //This is skipped if goto fullbreak3
						fullbreak3:

						meshingData.Add(new ChunkMeshingDataElement {
							pos = pos,
							size = boxSize
						});
					}

				voxelMask.Dispose();
			})
			.ScheduleParallel();

		Entities
			.WithAll<ChunkDirtyTag>()
			.WithNone<ChunkNotGeneratedTag>()
			.ForEach((
				ref DynamicBuffer<ChunkSubMeshData> subMeshBuffer,
				ref DynamicBuffer<VertexBufferElement> vertexBuffer,
				ref DynamicBuffer<NormalBufferElement> normalBuffer,
				ref DynamicBuffer<UVBufferElement> uvBuffer,
				ref DynamicBuffer<IndexBufferElement> indexBuffer,
				in DynamicBuffer<ChunkMeshingDataElement> meshingData
			) => {
				for (int i = 0; i < meshingData.Length; i++)
					AddBoxSurfaces(
						meshingData[i].pos, meshingData[i].size, blockTypes[0],
						subMeshBuffer,
						vertexBuffer,
						normalBuffer,
						uvBuffer,
						indexBuffer
					);
			})
			.WithDeallocateOnJobCompletion(blockTypes)
			.ScheduleParallel();

		Entities
			.WithAll<ChunkDirtyTag>()
			.WithNone<ChunkNotGeneratedTag>()
			.ForEach((
				ref PhysicsCollider collider,
				in DynamicBuffer<ChunkMeshingDataElement> meshingData
				) => {
				var shapes = new NativeList<CompoundCollider.ColliderBlobInstance>(Allocator.Temp);
				
				for (int i = 0; i < meshingData.Length; i++)
					shapes.Add(new CompoundCollider.ColliderBlobInstance {
						CompoundFromChild = new RigidTransform(quaternion.identity, meshingData[i].pos + meshingData[i].size * 0.5f),
						Collider = BoxCollider.Create(new BoxGeometry {
							Center = float3.zero,
							Orientation = quaternion.identity,
							Size = meshingData[i].size,
							BevelRadius = 0.5f
						})
					});

				collider.Value = CompoundCollider.Create(shapes);
				shapes.Dispose();
			})
			.ScheduleParallel();

		Entities
			.WithAll<ChunkDirtyTag>()
			.WithNone<ChunkNotGeneratedTag>()
			.ForEach((Entity e, int entityInQueryIndex, ref DynamicBuffer<ChunkMeshingDataElement> meshingData) => {
				meshingData.Clear();

				ecb.RemoveComponent<ChunkDirtyTag>(entityInQueryIndex, e);
				ecb.AddComponent<ChunkApplyMeshingTag>(entityInQueryIndex, e);
			})
			.ScheduleParallel();

		endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
	}

	static int Get3dIndex(int3 pos)
		=> pos.x + WorldData.CHUNK_SIZE * pos.y + WorldData.CHUNK_SIZE * WorldData.WORLD_HEIGHT * pos.z;

	static void AddBoxSurfaces(
		float3 origin, float3 size, BlockTypeData type,
		DynamicBuffer<ChunkSubMeshData> subMeshBuffer,
		DynamicBuffer<VertexBufferElement> vertexBuffer,
		DynamicBuffer<NormalBufferElement> normalBuffer,
		DynamicBuffer<UVBufferElement> uvBuffer,
		DynamicBuffer<IndexBufferElement> indexBuffer
	) {
		AddSurface(
			origin + new float3(0, 0, size.z),
			origin + new float3(0, size.y, size.z),
			origin + new float3(size.x, size.y, size.z),
			origin + new float3(size.x, 0, size.z),
			(int) size.x, (int) size.y,
			Direction.South, subMeshBuffer, GetOrCreateSubMeshIndex(subMeshBuffer, type.materialSouth, indexBuffer),
			vertexBuffer, normalBuffer, uvBuffer, indexBuffer
		);

		AddSurface(
			origin,
			origin + new float3(0, size.y, 0),
			origin + new float3(size.x, size.y, 0),
			origin + new float3(size.x, 0, 0),
			(int) size.x, (int) size.y,
			Direction.North, subMeshBuffer, GetOrCreateSubMeshIndex(subMeshBuffer, type.materialNorth, indexBuffer),
			vertexBuffer, normalBuffer, uvBuffer, indexBuffer
		);

		AddSurface(
			origin,
			origin + new float3(0, size.y, 0),
			origin + new float3(0, size.y, size.z),
			origin + new float3(0, 0, size.z),
			(int) size.z, (int) size.y,
			Direction.West, subMeshBuffer, GetOrCreateSubMeshIndex(subMeshBuffer, type.materialWest, indexBuffer),
			vertexBuffer, normalBuffer, uvBuffer, indexBuffer
		);

		AddSurface(
			origin + new float3(size.x, 0, 0),
			origin + new float3(size.x, size.y, 0),
			origin + new float3(size.x, size.y, size.z),
			origin + new float3(size.x, 0, size.z),
			(int) size.z, (int) size.y,
			Direction.East, subMeshBuffer, GetOrCreateSubMeshIndex(subMeshBuffer, type.materialEast, indexBuffer),
			vertexBuffer, normalBuffer, uvBuffer, indexBuffer
		);

		AddSurface(
			origin,
			origin + new float3(0, 0, size.z),
			origin + new float3(size.x, 0, size.z),
			origin + new float3(size.x, 0, 0),
			(int) size.x, (int) size.z,
			Direction.Bottom, subMeshBuffer, GetOrCreateSubMeshIndex(subMeshBuffer, type.materialBottom, indexBuffer),
			vertexBuffer, normalBuffer, uvBuffer, indexBuffer
		);

		AddSurface(
			origin + new float3(0, size.y, 0),
			origin + new float3(0, size.y, size.z),
			origin + new float3(size.x, size.y, size.z),
			origin + new float3(size.x, size.y, 0),
			(int) size.x, (int) size.z,
			Direction.Top, subMeshBuffer, GetOrCreateSubMeshIndex(subMeshBuffer, type.materialTop, indexBuffer),
			vertexBuffer, normalBuffer, uvBuffer, indexBuffer
		);
	}

	static int GetOrCreateSubMeshIndex(DynamicBuffer<ChunkSubMeshData> subMeshBuffer, int materialIndex, DynamicBuffer<IndexBufferElement> indexBuffer) {
		for (int j = 0; j < subMeshBuffer.Length; ++j)
			if (subMeshBuffer[j].blockType == materialIndex) {
				return j;
			}

		int subMeshIndex = subMeshBuffer.Add(new ChunkSubMeshData {
			blockType = materialIndex,
			indexOffset = indexBuffer.Length
		});
		return subMeshIndex;
	}

	static void AddSurface(
		float3 bottomLeft, float3 topLeft, float3 topRight, float3 bottomRight,
		int w, int h, Direction side, DynamicBuffer<ChunkSubMeshData> meshData, int subMeshIndex,
		DynamicBuffer<VertexBufferElement> vertexBuffer,
		DynamicBuffer<NormalBufferElement> normalBuffer,
		DynamicBuffer<UVBufferElement> uvBuffer,
		DynamicBuffer<IndexBufferElement> indexBuffer
	) {
		float3 normal;
		switch (side) {
			case Direction.North:
				normal = new float3(0, 0, -1);
				break;
			case Direction.South:
				normal = new float3(0, 0, 1);
				break;
			case Direction.East:
				normal = new float3(1, 0, 0);
				break;
			case Direction.West:
				normal = new float3(-1, 0, 0);
				break;
			case Direction.Top:
				normal = new float3(0, 1, 0);
				break;
			case Direction.Bottom:
				normal = new float3(0, -1, 0);
				break;
			default:
				throw new Exception("You gave a non existant direction.");
		}

		//if (wireframe) {
		/*int[] newIndices = new int[14];
		if (((int)side) % 2 == 0)
			newIndices = new int[] {2,3,1,2, 1,0,2,1};
		else
			newIndices = new int[] {2,0,1,2, 1,3,2,1};

		foreach (int newIndex in newIndices)
			surface.indices.Add(surface.vertices.Count + newIndex);*/
		//}else {

		var subMeshData = meshData[subMeshIndex];
		if (((int) side) % 2 == 0) {
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength, vertexBuffer.Length + 2);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 1, vertexBuffer.Length + 3);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 2, vertexBuffer.Length + 1);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 3, vertexBuffer.Length + 1);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 4, vertexBuffer.Length + 0);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 5, vertexBuffer.Length + 2);
		}
		else {
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength, vertexBuffer.Length + 2);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 1, vertexBuffer.Length + 0);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 2, vertexBuffer.Length + 1);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 3, vertexBuffer.Length + 1);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 4, vertexBuffer.Length + 3);
			indexBuffer.Insert(subMeshData.indexOffset + subMeshData.indexLength + 5, vertexBuffer.Length + 2);
		}

		subMeshData.indexLength += 6;
		meshData[subMeshIndex] = subMeshData;
		//}

		vertexBuffer.Add(bottomLeft);
		vertexBuffer.Add(bottomRight);
		vertexBuffer.Add(topLeft);
		vertexBuffer.Add(topRight);

		for (int i = 0; i < 4; ++i)
			normalBuffer.Add(normal);

		uvBuffer.Add(new float2(0, 0));
		uvBuffer.Add(new float2(w, 0));
		uvBuffer.Add(new float2(0, h));
		uvBuffer.Add(new float2(w, h));
	}
}