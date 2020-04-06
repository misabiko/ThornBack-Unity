using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
	public WorldData worldData;
	public BlockLibrary blockLibrary;

	int x, y;
	Dictionary<int, BlockLibrary.SurfaceData> surfaces;
	Mesh mesh;

	void Start() => Init(0, 0, ref worldData, ref blockLibrary);

	void Init(int x, int y, ref WorldData worldData, ref BlockLibrary blockLibrary) {
		this.x = x;
		this.y = y;
		surfaces = new Dictionary<int, BlockLibrary.SurfaceData>();

		worldData.tryInit(x, y);
		blockLibrary.Init();
		

		//transform.position = new Vector3(x, 0f, y) * CHUNK_SIZE;
		
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		GenerateMesh(ref worldData, ref blockLibrary);

		UpdateMesh();
	}

	void GenerateMesh(ref WorldData worldData, ref BlockLibrary blockLibrary) {
		//Clear mesh
		//Clear collider

		blockLibrary.AddBoxSurfaces(Vector3.zero, Vector3.one, blockLibrary.GetBlockType(worldData.getBlock(x, y, 0, 0, 0).type), ref surfaces);
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