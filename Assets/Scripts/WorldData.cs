using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WorldData : ScriptableObject {
	public const int CHUNK_SIZE = 16;
	public const int WORLD_HEIGHT = 128;
	public const int CHUNK_VOLUME = CHUNK_SIZE * WORLD_HEIGHT * CHUNK_SIZE;

	public struct BlockData {
		public int type;
		public bool rendered;

		public void Set(int type, bool rendered) {
			this.type = type;
			this.rendered = rendered;
		}

		bool Equals(BlockData other) => other.rendered == rendered && other.type == type;
	}

	public Dictionary<string, BlockData[]> chunks = new Dictionary<string, BlockData[]>();

	public static string GetChunkKey(int x, int y) => x + "," + y;

	public BlockData GetBlock(int chunkX, int chunkY, int x, int y, int z)
		=> chunks[GetChunkKey(chunkX, chunkY)][x + CHUNK_SIZE * y + CHUNK_SIZE * WORLD_HEIGHT * z];

	public void TryInit(int chunkX, int chunkY) {
		string coords = GetChunkKey(chunkX, chunkY);
		if (chunks.ContainsKey(coords)) return;
		
		chunks.Add(coords, new BlockData[CHUNK_VOLUME]);
		for (int i = 0; i < CHUNK_VOLUME; i++)
			chunks[coords][i] = new BlockData();

		chunks[coords][0].Set(1, true);
	}
}