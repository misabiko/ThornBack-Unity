using Unity.Entities;
using Unity.Mathematics;

public struct ChunkData : IComponentData {
	public int x, y;
}

public struct ChunkMeshData : IComponentData {
	public DynamicBuffer<ChunkSubMeshData> value;
}

public struct ChunkSubMeshData : IBufferElementData {
	public int blockType;
	public DynamicBuffer<VertexBufferElement> vertexBuffer;
	public DynamicBuffer<IndexBufferElement> indexBuffer;
}

public struct VertexBufferElement : IBufferElementData {
	public float3 vertex;
	public float3 normal;
	public float2 uv;

	public VertexBufferElement(float3 vertex, float3 normal, float2 uv) {
		this.vertex = vertex;
		this.normal = normal;
		this.uv = uv;
	}
}

public struct IndexBufferElement : IBufferElementData {
	public int value;
	
	public static implicit operator int(IndexBufferElement e) => e.value;

	public static implicit operator IndexBufferElement(int e) => new IndexBufferElement { value = e };
}

public struct ChunkDirtyTag : IComponentData {}

public struct ChunkApplyMeshingTag : IComponentData {}