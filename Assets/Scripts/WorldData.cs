using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WorldData : ScriptableObject {
	public const int CHUNK_SIZE = 16;
	public const int WORLD_HEIGHT = 128;
	public const int CHUNK_VOLUME = CHUNK_SIZE * WORLD_HEIGHT * CHUNK_SIZE;

	public class BlockData {
		public int type;
		public bool rendered;

		public void Set(int type, bool rendered) {
			this.type = type;
			this.rendered = rendered;
		}

		public bool Same(BlockData other) => other.rendered == rendered && other.type == type;

		public override string ToString() => $"BlockData: (Type: {type}, Rendered {rendered})";
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

		int chunkWorldX = chunkX * CHUNK_SIZE;
		int chunkWorldY = chunkY * CHUNK_SIZE;
		
		for (int x = 0; x < CHUNK_SIZE; x++)
			for (int z = 0; z < CHUNK_SIZE; z++) {
				Vector2 n = new Vector2((x + chunkWorldX) / 5.0f, (z + chunkWorldY) / 5.0f) / 20 - new Vector2(0.5f, 0.5f);
							
				float e1 = Ease((0.5f + Mathf.PerlinNoise(7.567f * n.x, 7.567f * n.y) / 2) + 0.02f, -8.57f);
				
				float e2 = Ease(((0.244f * Mathf.PerlinNoise(9.588f * n.x, 9.588f * n.y) * e1) + 1) / 2 + 0.041f, -3.36f) * 2 - 1;
				
				float e3 = Ease(((0.182f * Mathf.PerlinNoise(25.246f * n.x, 25.246f * n.y) * Mathf.Max(e1 - 0.433f, 0)) + 1) / 2 , -1.57f) * 2 - 1;
				
				float e = e2 + e3;

				int y = Mathf.FloorToInt(Mathf.Clamp((e + 3) * 15.3f, 0, WORLD_HEIGHT - 1));

				//if (x % 2 == 0 && z % 2 == 0) {
					GetBlock(chunkX, chunkY, x, y, z).Set(4, true);

					for (int j = 0; j < y; j++)
						GetBlock(chunkX, chunkY, x, j, z).Set(1, true);
				//}
			}
	}
	
	float Ease(float s, float curve) {
		if (s < 0)
			s = 0;
		else if (s > 1f)
			s = 1f;
		if (curve > 0)	{
			if (curve < 1f)
				return 1f - Mathf.Pow(1f - s, 1f / curve);
			else
				return Mathf.Pow(s, curve);
		} else if (curve < 0) {
			//inout ease

			if (s < 0.5)
				return Mathf.Pow(s * 2f, -curve) * 0.5f;
			else
				return (1f - Mathf.Pow(1f - (s - 0.5f) * 2f, -curve)) * 0.5f + 0.5f;
		} else
			return 0; // no ease (raw)
	}
}