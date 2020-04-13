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

		//Debug.Log("Pre : " + dirtyChunks.Length);
		foreach (Entity dirtyChunk in dirtyChunks) {
			Debug.Log("Dirty");
			RenderMesh renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(dirtyChunk);
			var mesh = renderMesh.mesh;

			mesh.Clear();
			//mesh.subMeshCount = blockLibrary.GetTypeCount();
			mesh.subMeshCount = 8;

			/*var vertices = new NativeList<float3>();
			var normals = new NativeList<float3>();
			var UVs = new NativeList<float2>();
			var indices = new NativeHashMap<int, NativeArray<int>>();

			int vertexCount = 0;
			foreach (var subMeshData in EntityManager.GetComponentData<ChunkMeshData>(dirtyChunk).value) {
				var subMeshIndices = subMeshData.indexBuffer.Reinterpret<int>();
				indices[subMeshData.blockType] = subMeshIndices.ToNativeArray(Allocator.Temp);

				foreach (VertexBufferElement vertexInfo in subMeshData.vertexBuffer) {
					vertices.Add(vertexInfo.vertex);
					normals.Add(vertexInfo.normal);
					UVs.Add(vertexInfo.uv);
				}

				vertexCount += subMeshData.vertexBuffer.Length;
			}*/
			
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

			/*var keyArray = indices.GetKeyArray(Allocator.Temp);
			foreach (int i in keyArray) {
				mesh.SetIndices(indices[i], MeshTopology.Triangles, i);
				indices[i].Dispose();
			}

			keyArray.Dispose();
			indices.Dispose();

			vertices.Dispose();
			normals.Dispose();
			UVs.Dispose();*/

			renderMesh.mesh = mesh;
			renderMesh.subMesh = 0;
			renderMesh.material = opaqueMaterial;
			EntityManager.SetSharedComponentData(dirtyChunk, renderMesh);
		}

		EntityManager.RemoveComponent<ChunkApplyMeshingTag>(dirtyChunkQuery);

		dirtyChunks.Dispose();
	}
}