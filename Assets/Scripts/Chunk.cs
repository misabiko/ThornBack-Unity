using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
	int x, y;
	Dictionary<int, BlockLibrary.SurfaceData> surfaces;
	Mesh mesh;

	public void Init(int x, int y, WorldData worldData, BlockLibrary blockLibrary) {
		this.x = x;
		this.y = y;
		surfaces = new Dictionary<int, BlockLibrary.SurfaceData>();

		Debug.Log("Init " + x + ", " + y);
		worldData.tryInit(x, y);
		blockLibrary.Init();
		
		transform.position = new Vector3(x, 0f, y) * WorldData.CHUNK_SIZE;
		
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		GenerateMesh(worldData, blockLibrary);

		UpdateMesh();
	}

	void GenerateMesh(WorldData worldData, BlockLibrary blockLibrary) {
		//Clear mesh
		//Clear collider

		blockLibrary.AddBoxSurfaces(Vector3.zero, Vector3.one, blockLibrary.GetBlockType(worldData.getBlock(x, y, 0, 0, 0).type), ref surfaces);
		blockLibrary.AddBoxSurfaces(Vector3.forward * 2, Vector3.one, blockLibrary.GetBlockType(worldData.getBlock(x, y, 0, 0, 0).type), ref surfaces);
	}

	void UpdateMesh() {
		mesh.Clear();

		foreach (KeyValuePair<int,BlockLibrary.SurfaceData> keyValuePair in surfaces) {
			mesh.vertices = keyValuePair.Value.vertices.ToArray();
			mesh.normals = keyValuePair.Value.normals.ToArray();
			mesh.uv = keyValuePair.Value.UVs.ToArray();
			mesh.triangles = keyValuePair.Value.indices.ToArray();
		}
	}
}