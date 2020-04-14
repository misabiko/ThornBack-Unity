using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ChunkLoader : MonoBehaviour {
	/*

	//public BlockLibrary blockLibrary;
	public GameObject chunkPrefab;
	public Transform player;

	Dictionary<string, Chunk> chunks;
	Queue<Tuple<int, int>> loadingBacklog;
	int lastChunkX, lastChunkY;*/

	EntityManager entityManager;
	EntityArchetype chunkArchetype;
	
	public int radius;
	public int preloadRadius;
	public WorldData worldData;

	public Material opaqueMaterial;

	void Start() {
		/*loadingBacklog = new Queue<Tuple<int, int>>();

		blockLibrary.Init();
		//worldData.load();

		float backlogSize = preloadRadius * preloadRadius * 4f;
		float numLoaded = 0;

		UpdateLoadingChunks();*/

		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

		//create entity
		var blockLibrary = entityManager.CreateEntity(typeof(BlockLibraryData), typeof(BlockTypeData));
		entityManager.SetName(blockLibrary, "BlockLibrary");

		//create singleton for BlockLibraryData
		var blockLibraryQuery = entityManager.CreateEntityQuery(typeof(BlockLibraryData));
		blockLibraryQuery.SetSingleton(new BlockLibraryData {});

		//create BlockMaterial entity
		var blockMaterial = entityManager.CreateEntity();
		entityManager.SetName(blockMaterial, "Opaque Material");
		entityManager.AddSharedComponentData(blockMaterial, new BlockMaterial {
			value = opaqueMaterial
		});

		//create BlockMaterialElement buffer
		var blockMaterialBuffer = entityManager.AddBuffer<BlockMaterialElement>(blockLibrary);
		int opaqueIndex = blockMaterialBuffer.Add(new BlockMaterialElement {
			blockMaterial = blockMaterial
		});

		//create BlockTypeMaterial entity
		/*var blockTypeMaterials = entityManager.CreateEntity();
		var blockTypeMaterialBuffer = entityManager.AddBuffer<BlockTypeMaterial>(blockTypeMaterials);
		for (int i = 0; i < 6; i++)
			blockTypeMaterialBuffer.Add(opaqueIndex);*/

		//create BlockTypeData buffer
		var blockTypes = entityManager.AddBuffer<BlockTypeData>(blockLibrary);
		blockTypes.Add(new BlockTypeData {
			index = 0,
			//materialBuffer = blockTypeMaterials
			materialSouth = 0,
			materialNorth = 0,
			materialWest = 0,
			materialEast = 0,
			materialBottom = 0,
			materialTop = 0
		});

		entityManager.SetName(entityManager.CreateEntity(typeof(WorldData)), "WorldData");
		var worldDataQuery = entityManager.CreateEntityQuery(typeof(WorldData));
		worldDataQuery.SetSingleton(new WorldData {
				CHUNK_SIZE = 16,
				WORLD_HEIGHT = 128
			});
		worldData = worldDataQuery.GetSingleton<WorldData>();
		

		chunkArchetype = entityManager.CreateArchetype(
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
			typeof(ChunkDirtyTag),
			typeof(ChunkNotGeneratedTag)
		);

		for (int x = -preloadRadius; x < preloadRadius; x++)
			for (int y = -preloadRadius; y < preloadRadius; y++)
				CreateNewChunk(x, y);
	}

	void CreateNewChunk(int x, int y) {
		var entity = entityManager.CreateEntity(chunkArchetype);
		entityManager.SetName(entity, $"Chunk {x}, {y}");
		
		entityManager.SetComponentData(entity, new ChunkData {
			x = x, y = y
		});
		entityManager.SetComponentData(entity, new Translation {
			Value = new float3(x * worldData.CHUNK_SIZE, 0, y * worldData.CHUNK_SIZE)
		});
		entityManager.SetSharedComponentData(entity, new RenderMesh() {
			mesh = new Mesh(),
			material = opaqueMaterial
		});
	}

	/*void UpdateLoadingChunks() {
		var loadingBacklogList = new List<Tuple<int, int>>();

		for (int x = lastChunkX - radius; x < lastChunkX + radius; x++)
		for (int y = lastChunkY - radius; y < lastChunkY + radius; y++) {
			if (
				Mathf.Pow(x - lastChunkX, 2) + Mathf.Pow(y - lastChunkY, 2) <= radius * radius &&
				!chunks.ContainsKey(WorldData.GetChunkKey(x, y))
			)
				loadingBacklogList.Add(Tuple.Create(x, y));
		}

		if (loadingBacklogList.Count == 0) return;

		loadingBacklogList.Sort();
		loadingBacklog = new Queue<Tuple<int, int>>(loadingBacklogList);
	}

	//TODO use distance from player's last pos
	static int CompareLoadingChunks(Tuple<int, int> a, Tuple<int, int> b)
		=> a.Item1 * a.Item1 + a.Item2 * a.Item2 - b.Item1 * b.Item1 + b.Item2 * b.Item2;

	void Update() {
		UpdatePlayerChunk();

		var chunkDataArray = new NativeArray<Chunk.Data>(loadingBacklog.Count, Allocator.TempJob);
		var job = new ChunkMesherJob();

		int i = 0;
		while (loadingBacklog.Count > 0) {
			Tuple<int, int> coords = loadingBacklog.Dequeue();

			Debug.Log("Loading " + WorldData.GetChunkKey(coords.Item1, coords.Item2));
			chunkDataArray[i++] = new Chunk.Data(CreateNewChunk(coords.Item1, coords.Item2));
		}

		var jobHandle = job.Schedule(chunkDataArray.Length, 1);
		jobHandle.Complete();
		chunkDataArray.Dispose();
	}

	void UpdatePlayerChunk() {
		var pos = player.position;
		int chunkX = Mathf.FloorToInt(pos.x / WorldData.CHUNK_SIZE);
		int chunkY = Mathf.FloorToInt(pos.z / WorldData.CHUNK_SIZE);

		if (chunkX == lastChunkX && chunkY == lastChunkY) return;

		lastChunkX = chunkX;
		lastChunkY = chunkY;
		UpdateLoadingChunks();
	}*/
}