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

		void Set(int type, bool rendered) {
			this.type = type;
			this.rendered = rendered;
		}

		bool equals(BlockData other) => other.rendered == rendered && other.type == type;
	}

	public Dictionary<string, BlockData[]> chunks = new Dictionary<string, BlockData[]>();

	public static string GetChunkKey(int x, int y) => x + "," + y;

	public BlockData getBlock(int chunkX, int chunkY, int x, int y, int z)
		=> chunks[GetChunkKey(chunkX, chunkY)][x + CHUNK_SIZE * y + CHUNK_SIZE * WORLD_HEIGHT * z];

	public void tryInit(int chunkX, int chunkY) {
		string coords = GetChunkKey(chunkX, chunkY);
		if (chunks.ContainsKey(coords)) return;
		
		chunks.Add(coords, new BlockData[CHUNK_VOLUME]);
		chunks[coords][0] = new BlockData() {type = 1, rendered = true};
	}
}