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
			
			var meshData = EntityManager.GetBuffer<ChunkSubMeshData>(dirtyChunk);

			mesh.Clear();
			mesh.subMeshCount = meshData.Length;
			
			mesh.SetVertices(EntityManager.GetBuffer<VertexBufferElement>(dirtyChunk).AsNativeArray());
			mesh.SetNormals(EntityManager.GetBuffer<NormalBufferElement>(dirtyChunk).AsNativeArray());
			mesh.SetUVs(0, EntityManager.GetBuffer<UVBufferElement>(dirtyChunk).AsNativeArray());

			var indexBuffer = EntityManager.GetBuffer<IndexBufferElement>(dirtyChunk);
			for (int i = 0; i < meshData.Length; i++) {
				var indices = new NativeArray<int>(meshData[i].indexLength - meshData[i].indexOffset, Allocator.Temp);
				for (int j = 0; j < meshData[i].indexLength - meshData[i].indexOffset; j++)
					indices[j] = indexBuffer[meshData[i].indexOffset + j];

				mesh.SetIndices(indices, MeshTopology.Triangles, i);
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