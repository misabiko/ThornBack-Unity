using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
	[HideInInspector]
	public int x, y;
	Dictionary<int, BlockLibrary.SurfaceData> surfaces;
	Mesh mesh;

	public void Init(int x, int y, WorldData worldData, BlockLibrary blockLibrary) {
		this.x = x;
		this.y = y;
		surfaces = new Dictionary<int, BlockLibrary.SurfaceData>();

		worldData.TryInit(x, y);
		blockLibrary.Init();
		
		transform.position = new Vector3(x, 0f, y) * WorldData.CHUNK_SIZE;
		
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		GenerateMesh(worldData, blockLibrary);

		UpdateMesh(blockLibrary);
	}

	void GenerateMesh(WorldData worldData, BlockLibrary blockLibrary) {
		//Clear mesh
		//Clear collider
		
		{
			foreach (KeyValuePair<int,BlockLibrary.SurfaceData> keyValuePair in surfaces)
				keyValuePair.Value.Clear();
		
			/*Array owners = staticBody.Get_shape_owners();
			for (int i = 0; i < owners.size(); i++)
				staticBody.shape_owner_clear_shapes(owners[i]);*/
		}

		bool[,,] voxelMask = new bool[WorldData.CHUNK_SIZE, WorldData.WORLD_HEIGHT, WorldData.CHUNK_SIZE];

		Vector3 pos, cubeSize;
		for (int i = 0; i < WorldData.CHUNK_SIZE; i++)
			for (int j = 0; j < WorldData.WORLD_HEIGHT; j++)
				for (int k = 0; k < WorldData.CHUNK_SIZE; k++)
					if (!voxelMask[i, j, k] && worldData.GetBlock(x, y, i, j, k).type > 0) {
						voxelMask[i, j, k] = true;
						cubeSize = new Vector3(1, 1, 1);

						for (int di = 1; di < WorldData.CHUNK_SIZE - i; di++)
							if (voxelMask[i + di, j, k] || !worldData.GetBlock(x, y, i + di, j, k).Same(worldData.GetBlock(x, y, i, j, k))) {
								cubeSize.x += di - 1;
								goto fullbreak1;
							}else
								voxelMask[i + di, j, k] = true;
						
						cubeSize.x += WorldData.CHUNK_SIZE - i - 1;	//This is skipped if goto fullbreak1
						fullbreak1:

						for (int dk = 1; dk < WorldData.CHUNK_SIZE - k; dk++) {
							for (int di = 0; di < cubeSize.x; di++)
								if (voxelMask[i + di, j, k + dk] || !worldData.GetBlock(x, y, i + di, j, k + dk).Same(worldData.GetBlock(x, y, i, j, k))) {
									cubeSize.z += dk - 1;
									goto fullbreak2;
								}
							
							for (int di = 0; di < cubeSize.x; di++)
								voxelMask[i + di, j, k + dk] = true;
						}

						cubeSize.z += WorldData.CHUNK_SIZE - k - 1;	//This is skipped if goto fullbreak2
						fullbreak2:

						for (int dj = 1; dj < WorldData.WORLD_HEIGHT - j; dj++) {
							for (int dk = 0; dk < cubeSize.z; dk++)
								for (int di = 0; di < cubeSize.x; di++)
									if (voxelMask[i + di, j + dj, k + dk] || !worldData.GetBlock(x, y, i + di, j + dj, k + dk).Same(worldData.GetBlock(x, y, i, j, k))) {
										cubeSize.y += dj - 1;
										goto fullbreak3;
									}
							
							for (int dk = 0; dk < cubeSize.z; dk++)
								for (int di = 0; di < cubeSize.x; di++)
									voxelMask[i + di, j + dj, k + dk] = true;
						}

						cubeSize.y += WorldData.WORLD_HEIGHT - j - 1;	//This is skipped if goto fullbreak3
						fullbreak3:

						pos = new Vector3(i, j, k);

						/*Ref<BoxShape> box;
						box.instance();
						box.set_extents(cubeSize * 0.5f);
						int64_t shapeOwner = staticBody.create_shape_owner(staticBody);

						staticBody.shape_owner_set_disabled(shapeOwner, true);
						staticBody.shape_owner_set_transform(shapeOwner, Transform().translated(pos + cubeSize * 0.5f));
						staticBody.shape_owner_add_shape(shapeOwner, box);*/

						blockLibrary.AddBoxSurfaces(pos, cubeSize, blockLibrary.GetBlockType(worldData.GetBlock(x, y, i, j, k).type), ref surfaces);
					}
	}

	void UpdateMesh(BlockLibrary blockLibrary) {
		mesh.Clear();
		mesh.subMeshCount = blockLibrary.GetTypeCount();

		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		var UVs = new List<Vector2>();

		int vertexCount = 0;
		
		foreach (KeyValuePair<int,BlockLibrary.SurfaceData> keyValuePair in surfaces) {
			for (int i = 0; i < keyValuePair.Value.indices.Count; ++i)
				keyValuePair.Value.indices[i] += vertexCount;
			
			vertices.AddRange(keyValuePair.Value.vertices);
			normals.AddRange(keyValuePair.Value.normals);
			UVs.AddRange(keyValuePair.Value.UVs);
			vertexCount += keyValuePair.Value.vertices.Count;
		} 
		
		mesh.SetVertices(vertices);
		mesh.SetNormals(normals);
		mesh.SetUVs(0, UVs);

		foreach (KeyValuePair<int, BlockLibrary.SurfaceData> keyValuePair in surfaces)
			mesh.SetIndices(keyValuePair.Value.indices, MeshTopology.Triangles, keyValuePair.Key);
	}
}