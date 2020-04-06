using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkLoader : MonoBehaviour {
    public int radius;
    public int preloadRadius;
    public WorldData worldData;
    public BlockLibrary blockLibrary;
    public GameObject chunkPrefab;

    Dictionary<string, Chunk> chunks;
    Queue<Tuple<int, int>> loadingBacklog;

    void Start() {
        //worldData.load();
        
        chunks = new Dictionary<string, Chunk>();
        loadingBacklog = new Queue<Tuple<int, int>>();
        
        UpdateLoadingChunks(0, 0);
    }

    void CreateNewChunk(int x, int y) {
        GameObject chunkGameObject = Instantiate(chunkPrefab, transform);
        
        Chunk chunk = chunkGameObject.GetComponent<Chunk>();
        chunk.Init(x, y, worldData, blockLibrary);
        
        chunkGameObject.name = WorldData.GetChunkKey(x, y);
        chunks.Add(chunkGameObject.name, chunk);
    }

    void UpdateLoadingChunks(int chunkX, int chunkY) {
        var loadingBacklogList = new List<Tuple<int, int>>();
        
        for (int x = chunkX - radius; x < chunkX + radius; x++)
            for (int y = chunkY - radius; y < chunkY + radius; y++) {
                if (
                    Mathf.Pow(x - chunkX, 2) + Mathf.Pow(y - chunkY, 2) <= radius * radius &&
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
}
