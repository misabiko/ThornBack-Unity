using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour {
	public int radius;
	public int preloadRadius;
	public WorldData worldData;
	public BlockLibrary blockLibrary;
	public GameObject chunkPrefab;
	public Transform player;

	Dictionary<string, Chunk> chunks;
	Queue<Tuple<int, int>> loadingBacklog;
	int lastChunkX, lastChunkY;

	void Start() {
		chunks = new Dictionary<string, Chunk>();
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

		UpdateLoadingChunks();
	}

	void CreateNewChunk(int x, int y) {
		GameObject chunkGameObject = Instantiate(chunkPrefab, transform);

		Chunk chunk = chunkGameObject.GetComponent<Chunk>();
		chunk.Init(x, y, worldData, blockLibrary);

		chunkGameObject.name = WorldData.GetChunkKey(x, y);
		chunks.Add(chunkGameObject.name, chunk);
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
		
		if (HasBacklog())
			LoadChunks();
	}

	bool HasBacklog() => loadingBacklog.Count > 0;

	void LoadChunks() {
		while (HasBacklog()) {
			Tuple<int, int> coords = loadingBacklog.Dequeue();

			Debug.Log("Loading " + WorldData.GetChunkKey(coords.Item1, coords.Item2));
			CreateNewChunk(coords.Item1, coords.Item2);
		}
	}

	void UpdatePlayerChunk() {
		var pos = player.position;
		int chunkX = Mathf.FloorToInt(pos.x / WorldData.CHUNK_SIZE);
		int chunkY = Mathf.FloorToInt(pos.z / WorldData.CHUNK_SIZE);

		if (chunkX == lastChunkX && chunkY == lastChunkY) return;

		lastChunkX = chunkX;
		lastChunkY = chunkY;
		UpdateLoadingChunks();
	}
}