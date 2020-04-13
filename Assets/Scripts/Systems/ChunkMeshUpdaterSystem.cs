using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class ChunkMeshUpdaterSystem : SystemBase {
	protected override void OnUpdate() {
		var dirtyChunkQuery = GetEntityQuery(typeof(ChunkApplyMeshingTag));
		var dirtyChunks = dirtyChunkQuery.ToEntityArray(Allocator.TempJob);

		foreach (Entity dirtyChunk in dirtyChunks) {
			RenderMesh renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(dirtyChunk);
			var mesh = renderMesh.mesh;

			mesh.Clear();
			//mesh.subMeshCount = blockLibrary.GetTypeCount();
			mesh.subMeshCount = 8;

			var vertices = new NativeList<float3>();
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
			}

			mesh.SetVertices(vertices.AsArray());
			mesh.SetNormals(normals.AsArray());
			mesh.SetUVs(0, UVs.AsArray());

			var keyArray = indices.GetKeyArray(Allocator.Temp);
			foreach (int i in keyArray) {
				mesh.SetIndices(indices[i], MeshTopology.Triangles, i);
				indices[i].Dispose();
			}

			keyArray.Dispose();
			indices.Dispose();

			vertices.Dispose();
			normals.Dispose();
			UVs.Dispose();
		}

		EntityManager.RemoveComponent<ChunkApplyMeshingTag>(dirtyChunkQuery);

		dirtyChunks.Dispose();
	}
}