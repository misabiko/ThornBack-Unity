using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ChunkLoader : MonoBehaviour {
	public int radius;
	public int preloadRadius;

	public WorldData worldData;

	//public BlockLibrary blockLibrary;
	public GameObject chunkPrefab;
	public Transform player;

	Dictionary<string, Chunk> chunks;
	Queue<Tuple<int, int>> loadingBacklog;
	int lastChunkX, lastChunkY;

	EntityManager entityManager;
	EntityArchetype chunkArchetype;

	public Material opaqueMaterial;

	void Start() {
		/*chunks = new Dictionary<string, Chunk>();
		loadingBacklog = new Queue<Tuple<int, int>>();

		blockLibrary.Init();
		//worldData.load();

		float backlogSize = preloadRadius * preloadRadius * 4f;
		float numLoaded = 0;

		for (int x = -preloadRadius; x < preloadRadius; x++) {
			for (int y = -preloadRadius; y < preloadRadius; y++) {
				CreateNewChunk(x, y);
				numLoaded++;
			}

			Debug.Log((numLoaded / backlogSize) * 100 + "%");
		}

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

		entityManager.CreateEntity(typeof(WorldData));
		entityManager.CreateEntityQuery(typeof(WorldData))
			.SetSingleton(new WorldData {
				CHUNK_SIZE = 16,
				WORLD_HEIGHT = 128
			});

		chunkArchetype = entityManager.CreateArchetype(
			typeof(ChunkData),
			typeof(Translation),
			typeof(Rotation),
			typeof(RenderMesh),
			typeof(RenderBounds),
			typeof(LocalToWorld)
		);

		var entity = entityManager.CreateEntity(chunkArchetype);
		entityManager.SetComponentData(entity, new ChunkData {
			x = 0, y = 0
		});
		entityManager.SetSharedComponentData(entity, new RenderMesh() {
			mesh = new Mesh(),
			material = opaqueMaterial
		});

		entityManager.AddBuffer<ChunkSubMeshData>(entity);
		entityManager.AddBuffer<VertexBufferElement>(entity);
		entityManager.AddBuffer<NormalBufferElement>(entity);
		entityManager.AddBuffer<UVBufferElement>(entity);
		entityManager.AddBuffer<IndexBufferElement>(entity);
		entityManager.AddBuffer<WorldBlockData>(entity);

		entityManager.AddComponentData(entity, new ChunkDirtyTag());
		entityManager.AddComponentData(entity, new ChunkNotGeneratedTag());
	}

	/*Chunk CreateNewChunk(int x, int y) {
		GameObject chunkGameObject = Instantiate(chunkPrefab, transform);

		Chunk chunk = chunkGameObject.GetComponent<Chunk>();
		chunk.Init(x, y, worldData, blockLibrary);

		chunkGameObject.name = WorldData.GetChunkKey(x, y);
		chunks.Add(chunkGameObject.name, chunk);

		return chunk;
	}

	void UpdateLoadingChunks() {
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