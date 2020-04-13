/*[CreateAssetMenu]
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
}*/

using Unity.Entities;
using Unity.Mathematics;

//TODO make those const/defined
public struct WorldData : IComponentData {
	public int CHUNK_SIZE;
	public int WORLD_HEIGHT;
}

[InternalBufferCapacity(0)]
public struct WorldBlockData : IBufferElementData {
	public int type;
}

public struct ChunkNotGeneratedTag : IComponentData {}