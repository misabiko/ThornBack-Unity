using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class ChunkMeshUpdaterSystem : SystemBase {
	protected override void OnUpdate() {
		var blockLibrary = GetSingletonEntity<BlockLibraryData>();
		var blockMaterial = EntityManager.GetBuffer<BlockMaterialElement>(blockLibrary)[0].blockMaterial;
		var opaqueMaterial = EntityManager.GetSharedComponentData<BlockMaterial>(blockMaterial);
		
		var dirtyChunkQuery = GetEntityQuery(typeof(ChunkApplyMeshingTag));
		var dirtyChunks = dirtyChunkQuery.ToEntityArray(Allocator.TempJob);

		foreach (Entity dirtyChunk in dirtyChunks) {
			RenderMesh renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(dirtyChunk);
			Mesh mesh = renderMesh.mesh;

			mesh.Clear();
			//mesh.subMeshCount = blockLibrary.GetTypeCount();
			mesh.subMeshCount = 8;
			
			mesh.SetVertices(EntityManager.GetBuffer<VertexBufferElement>(dirtyChunk).AsNativeArray());
			mesh.SetNormals(EntityManager.GetBuffer<NormalBufferElement>(dirtyChunk).AsNativeArray());
			mesh.SetUVs(0, EntityManager.GetBuffer<UVBufferElement>(dirtyChunk).AsNativeArray());

			var indexBuffer = EntityManager.GetBuffer<IndexBufferElement>(dirtyChunk);
			foreach (ChunkSubMeshData subMeshData in EntityManager.GetBuffer<ChunkSubMeshData>(dirtyChunk)) {
				var indices = new NativeArray<int>(subMeshData.indexLength - subMeshData.indexOffset, Allocator.Temp);
				for (int i = 0; i < subMeshData.indexLength - subMeshData.indexOffset; i++)
					indices[i] = indexBuffer[subMeshData.indexOffset + i];
				
				mesh.SetIndices(indices, MeshTopology.Triangles, subMeshData.blockType);
				indices.Dispose();
			}

			EntityManager.SetComponentData(dirtyChunk, new RenderBounds {
				Value = mesh.bounds.ToAABB()
			});
		}

		EntityManager.RemoveComponent<ChunkApplyMeshingTag>(dirtyChunkQuery);

		dirtyChunks.Dispose();
	}
}