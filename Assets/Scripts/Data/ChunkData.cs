using Unity.Entities;
using Unity.Mathematics;

public struct ChunkData : IComponentData {
	public int x, y;
}

/*public struct ChunkMeshData : IComponentData {
	public DynamicBuffer<ChunkSubMeshData> value;
	public int boo;
}*/

public struct ChunkSubMeshData : IBufferElementData {
	public int blockType;
	public int indexOffset;
	public int indexLength;
}

public struct VertexBufferElement : IBufferElementData {
	public float3 value;
	
	public static implicit operator float3(VertexBufferElement e) => e.value;

	public static implicit operator VertexBufferElement(float3 e) => new VertexBufferElement { value = e };
}

public struct NormalBufferElement : IBufferElementData {
	public float3 value;
	
	public static implicit operator float3(NormalBufferElement e) => e.value;

	public static implicit operator NormalBufferElement(float3 e) => new NormalBufferElement { value = e };
}

public struct UVBufferElement : IBufferElementData {
	public float2 value;
	
	public static implicit operator float2(UVBufferElement e) => e.value;

	public static implicit operator UVBufferElement(float2 e) => new UVBufferElement { value = e };
}

public struct IndexBufferElement : IBufferElementData {
	public int value;
	
	public static implicit operator int(IndexBufferElement e) => e.value;

	public static implicit operator IndexBufferElement(int e) => new IndexBufferElement { value = e };
}

public struct ChunkDirtyTag : IComponentData {}

public struct ChunkApplyMeshingTag : IComponentData {}