using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour {
    public int radius;
    public int preloadRadius;
    public WorldData worldData;
    public BlockLibrary blockLibrary;
    public GameObject chunkPrefab;

    Dictionary<string, Chunk> chunks;

    void Start() {
        //worldData.load();
        
        chunks = new Dictionary<string, Chunk>();

        GameObject chunkGameObject = Instantiate(chunkPrefab, transform);
        
        Chunk chunk = chunkGameObject.GetComponent<Chunk>();
        chunk.Init(0, 0, ref worldData, ref blockLibrary);
        
        chunkGameObject.name = GetChunkKey(0, 0);
        chunks.Add(chunkGameObject.name, chunk);
    }

    static string GetChunkKey(int x, int y) => x + "," + y;

    void Update() {
        
    }
}
