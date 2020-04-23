using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ChunkLoader : MonoBehaviour {
	/*
	Queue<Tuple<int, int>> loadingBacklog;
	*/
	
	public int radius;
	public int preloadRadius;
	public WorldData worldData;
	public Material opaqueMaterial;
	public Transform player;

	EntityManager entityManager;
	EntityArchetype chunkArchetype;
	int lastChunkX, lastChunkY;

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
		worldDataQuery.SetSingleton(new WorldData());
		worldData = worldDataQuery.GetSingleton<WorldData>();

		var loadingQueueQuery = entityManager.CreateEntityQuery(typeof(ChunkLoaderQueueElement));
		var loadingBuffer = entityManager.GetBuffer<ChunkLoaderQueueElement>(loadingQueueQuery.GetSingletonEntity());
		for (int x = -preloadRadius; x < preloadRadius; x++)
		for (int y = -preloadRadius; y < preloadRadius; y++)
			loadingBuffer.Add(new int2(x, y));
		loadingQueueQuery.Dispose();
	}

	//TODO use distance from player's last pos
	/*static int CompareLoadingChunks(Tuple<int, int> a, Tuple<int, int> b)
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
	}*/
}