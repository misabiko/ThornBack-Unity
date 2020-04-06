using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WorldData : ScriptableObject {
	public const int CHUNK_SIZE = 16;
	public const int WORLD_HEIGHT = 128;

	public struct BlockData {
		public int type;
		public bool rendered;

		void Set(int type, bool rendered) {
			this.type = type;
			this.rendered = rendered;
		}

		bool equals(BlockData other) => other.rendered == rendered && other.type == type;
	}

	public Dictionary<Tuple<int, int>, List<BlockData>> chunks = new Dictionary<Tuple<int, int>, List<BlockData>>();

	public BlockData getBlock(int chunkX, int chunkY, int x, int y, int z)
		=> chunks[Tuple.Create<int, int>(chunkX, chunkY)][x + CHUNK_SIZE * y + CHUNK_SIZE * WORLD_HEIGHT * z];

	public void tryInit(int chunkX, int chunkY) {
		var coords = Tuple.Create(chunkX, chunkY);
		chunks.Add(coords, new List<BlockData>());
		chunks[coords].Add(new BlockData() {type = 1, rendered = true});
	}
}