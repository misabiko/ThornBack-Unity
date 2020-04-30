using Unity.Entities;

public struct PlayerChunkCoord : IComponentData {
	public int chunkX;
	public int chunkY;
}

public struct PlayerInputData : IComponentData {
	public bool pause;
}