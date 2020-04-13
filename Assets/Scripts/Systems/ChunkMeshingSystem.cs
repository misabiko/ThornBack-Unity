using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
		RequireSingletonForUpdate<WorldData>();
	}

	protected override void OnUpdate() {
		var blockLibrary = GetSingletonEntity<BlockLibraryData>();
		var blockTypes = EntityManager.GetBuffer<BlockTypeData>(blockLibrary).ToNativeArray(Allocator.TempJob);

		var worldData = GetSingleton<WorldData>();
		//var worldBlockBufferEntities = GetEntityQuery(ComponentType.ReadOnly<WorldBlockData>()).ToEntityArray(Allocator.TempJob);
		//var worldBlockBuffers = GetBufferFromEntity<ComponentType.ReadOnly<WorldBlockData>>();

		//ToConcurrent makes it so we can use it in parallel jobs
		EntityCommandBuffer.Concurrent ecb = endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
		
		NativeArray<bool> voxelMask = new NativeArray<bool>(worldData.CHUNK_SIZE * worldData.WORLD_HEIGHT * worldData.CHUNK_SIZE, Allocator.TempJob);

		Entities
			.WithAll<ChunkDirtyTag>()
			.WithNone<ChunkNotGeneratedTag>()
			.ForEach((
				Entity e, int entityInQueryIndex,
				ref DynamicBuffer<ChunkSubMeshData> subMeshBuffer,
				ref DynamicBuffer<VertexBufferElement> vertexBuffer,
				ref DynamicBuffer<NormalBufferElement> normalBuffer,
				ref DynamicBuffer<UVBufferElement> uvBuffer,
				ref DynamicBuffer<IndexBufferElement> indexBuffer,
				in DynamicBuffer<WorldBlockData> worldBlockBuffer
			) => {
				for (int i = 0; i < voxelMask.Length; i++)
					voxelMask[i] = false;

				float3 pos, cubeSize;
				for (int i = 0; i < worldData.CHUNK_SIZE; i++)
				for (int j = 0; j < worldData.WORLD_HEIGHT; j++)
				for (int k = 0; k < worldData.CHUNK_SIZE; k++)
					if (!voxelMask[Get3dIndex(worldData, i, j, k)] && GetBlockType(worldBlockBuffer, worldData,i, j, k) > 0) {
						voxelMask[Get3dIndex(worldData, i, j, k)] = true;
						cubeSize = new float3(1, 1, 1);

						for (int di = 1; di < worldData.CHUNK_SIZE - i; di++)
							if (voxelMask[Get3dIndex(worldData, i + di, j, k)] || (GetBlockType(worldBlockBuffer, worldData,i + di, j, k) != GetBlockType(worldBlockBuffer, worldData,i, j, k))) {
								cubeSize.x += di - 1;
								goto fullbreak1;
							}
							else
								voxelMask[Get3dIndex(worldData, i + di, j, k)] = true;

						cubeSize.x += worldData.CHUNK_SIZE - i - 1; //This is skipped if goto fullbreak1
						fullbreak1:

						for (int dk = 1; dk < worldData.CHUNK_SIZE - k; dk++) {
							for (int di = 0; di < cubeSize.x; di++)
								if (voxelMask[Get3dIndex(worldData, i + di, j, k + dk)] || (GetBlockType(worldBlockBuffer, worldData,i + di, j, k + dk) != GetBlockType(worldBlockBuffer, worldData,i, j, k))) {
									cubeSize.z += dk - 1;
									goto fullbreak2;
								}

							for (int di = 0; di < cubeSize.x; di++)
								voxelMask[Get3dIndex(worldData, i + di, j, k + dk)] = true;
						}

						cubeSize.z += worldData.CHUNK_SIZE - k - 1; //This is skipped if goto fullbreak2
						fullbreak2:

						for (int dj = 1; dj < worldData.WORLD_HEIGHT - j; dj++) {
							for (int dk = 0; dk < cubeSize.z; dk++)
							for (int di = 0; di < cubeSize.x; di++)
								if (voxelMask[Get3dIndex(worldData, i + di, j + dj, k + dk)] || (GetBlockType(worldBlockBuffer, worldData,i + di, j + dj, k + dk) != GetBlockType(worldBlockBuffer, worldData,i, j, k))) {
									cubeSize.y += dj - 1;
									goto fullbreak3;
								}

							for (int dk = 0; dk < cubeSize.z; dk++)
							for (int di = 0; di < cubeSize.x; di++)
								voxelMask[Get3dIndex(worldData, i + di, j + dj, k + dk)] = true;
						}

						cubeSize.y += worldData.WORLD_HEIGHT - j - 1; //This is skipped if goto fullbreak3
						fullbreak3:

						pos = new float3(i, j, k);

						/*var box = gameObject.AddComponent<BoxCollider>();
						box.size = cubeSize;
						box.center = pos + cubeSize * 0.5f;*/

						AddBoxSurfaces(pos, cubeSize, blockTypes[0], subMeshBuffer, vertexBuffer, normalBuffer, uvBuffer, indexBuffer);
					}
				
				
				ecb.RemoveComponent<ChunkDirtyTag>(entityInQueryIndex, e);
				ecb.AddComponent<ChunkApplyMeshingTag>(entityInQueryIndex, e);
			})
			.WithDeallocateOnJobCompletion(blockTypes)
			.WithDeallocateOnJobCompletion(voxelMask)
			.ScheduleParallel();

		endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
	}

	static int Get3dIndex(WorldData worldData, int x, int y, int z)
		=> x + worldData.CHUNK_SIZE * y + worldData.CHUNK_SIZE * worldData.WORLD_HEIGHT * z;

	static int GetBlockType(DynamicBuffer<WorldBlockData> worldBlockBuffer, WorldData worldData, int x, int y, int z)
		=> worldBlockBuffer[Get3dIndex(worldData, x, y, z)].type;

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