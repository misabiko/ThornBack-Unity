using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BlockLibrary : ScriptableObject {
	enum Direction {
		NORTH = 0,
		SOUTH = 1,
		EAST = 2,
		WEST = 3,
		TOP = 4,
		BOTTOM = 5
	};

	public class SurfaceData {
		public List<Vector3> vertices;
		public List<Vector3> normals;
		public List<Vector2> UVs;
		public List<int> indices;

		public SurfaceData() => Clear();

		void Clear() {
			vertices = new List<Vector3>();
			normals = new List<Vector3>();
			UVs = new List<Vector2>();
			indices = new List<int>();
		}

		void Add(SurfaceData surface) {
			//int vertexCount = 
		}
	}

	public struct TypeData {
		public string name;

		public int[] materials;
		public bool multiTextured;

		//public bool opaque;
		//bool colored;

		public TypeData(string name, int texture) {
			this.name = name;
			multiTextured = false;

			materials = new int[6];
			for (int i = 0; i < 6; ++i)
				materials[i] = texture;
		}

		public TypeData(string name, int texSouth, int texNorth, int texWest, int texEast, int texBottom, int texTop) {
			this.name = name;
			multiTextured = true;

			materials = new[] {
				texSouth,
				texNorth,
				texWest,
				texEast,
				texBottom,
				texTop
			};
		}
	}

	List<TypeData> types = new List<TypeData>();

	public void Init() {
		types.Add(new TypeData("Dirt", 0));					//1
		types.Add(new TypeData("Stone", 1));					//2
		types.Add(new TypeData("Cobblestone", 2));			//3
		types.Add(new TypeData("Grass",
			4, 4,
			4, 4,
			0, 3
			));	//4
		//Color(0, 0.52, 0.125, 1)
		types.Add(new TypeData("Sand", 5));					//5
		types.Add(new TypeData("Gravel", 6));				//6
		types.Add(new TypeData("Wool", 7));					//7
	}

	public TypeData GetBlockType(int typeId) {
		Debug.Log(typeId);
		return types[typeId - 1];
	}

	void AddSurface(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, int w, int h, Direction side, SurfaceData surface) {
		Vector3 normal;
		switch (side) {
			case Direction.NORTH:
				normal = new Vector3(0, 0, -1);
				break;
			case Direction.SOUTH:
				normal = new Vector3(0, 0, 1);
				break;
			case Direction.EAST:
				normal = new Vector3(1, 0, 0);
				break;
			case Direction.WEST:
				normal = new Vector3(-1, 0, 0);
				break;
			case Direction.TOP:
				normal = new Vector3(0, 1, 0);
				break;
			case Direction.BOTTOM:
				normal = new Vector3(0, -1, 0);
				break;
			default:
				throw new Exception("You gave a non existant direction.");
		}

		/* if (wireframe) {
			std::array<int, 14> newIndices;
			if (side % 2)
				newIndices = {2,3,1,2, 1,0,2,1};
			else
				newIndices = {2,0,1,2, 1,3,2,1};
	
			for (const unsigned& i : newIndices)
				surface->second.indices.push_back(surface->second.vertices.size() + i);
		}else { */
		int[] newIndices;
		if (((int) side) % 2 == 1)
			newIndices = new[] {2, 3, 1, 1, 0, 2};
		else
			newIndices = new[] {2, 0, 1, 1, 3, 2};

		foreach (int newIndex in newIndices)
			surface.indices.Add(surface.vertices.Count + newIndex);
		//}

		for (int i = 0; i < 4; i++)
			surface.normals.Add(normal);

		surface.UVs.Add(new Vector2(0, h));
		surface.UVs.Add(new Vector2(w, h));
		surface.UVs.Add(new Vector2(0, 0));
		surface.UVs.Add(new Vector2(w, 0));

		surface.vertices.Add(bottomLeft);
		surface.vertices.Add(bottomRight);
		surface.vertices.Add(topLeft);
		surface.vertices.Add(topRight);
	}

	public void AddBoxSurfaces(Vector3 origin, Vector3 size, TypeData type, ref Dictionary<int, SurfaceData> surfaces) {
		for (int i = 0; i < 6; i++)
			if (!surfaces.ContainsKey(type.materials[i]))
				surfaces.Add(type.materials[i], new SurfaceData());

		AddSurface(
			origin + new Vector3(0, 0, size.z),
			origin + new Vector3(0, size.y, size.z),
			origin + new Vector3(size.x, size.y, size.z),
			origin + new Vector3(size.x, 0, size.z),
			(int) size.x, (int) size.y,
			Direction.SOUTH, surfaces[type.materials[0]]
		);

		AddSurface(
			origin,
			origin + new Vector3(0, size.y, 0),
			origin + new Vector3(size.x, size.y, 0),
			origin + new Vector3(size.x, 0, 0),
			(int) size.x, (int) size.y,
			Direction.NORTH, surfaces[type.materials[1]]
		);

		AddSurface(
			origin,
			origin + new Vector3(0, size.y, 0),
			origin + new Vector3(0, size.y, size.z),
			origin + new Vector3(0, 0, size.z),
			(int) size.z, (int) size.y,
			Direction.WEST, surfaces[type.materials[2]]
		);

		AddSurface(
			origin + new Vector3(size.x, 0, 0),
			origin + new Vector3(size.x, size.y, 0),
			origin + new Vector3(size.x, size.y, size.z),
			origin + new Vector3(size.x, 0, size.z),
			(int) size.z, (int) size.y,
			Direction.EAST, surfaces[type.materials[3]]
		);

		AddSurface(
			origin,
			origin + new Vector3(0, 0, size.z),
			origin + new Vector3(size.x, 0, size.z),
			origin + new Vector3(size.x, 0, 0),
			(int) size.x, (int) size.z,
			Direction.BOTTOM, surfaces[type.materials[4]]
		);

		AddSurface(
			origin + new Vector3(0, size.y, 0),
			origin + new Vector3(0, size.y, size.z),
			origin + new Vector3(size.x, size.y, size.z),
			origin + new Vector3(size.x, size.y, 0),
			(int) size.x, (int) size.z,
			Direction.TOP, surfaces[type.materials[5]]
		);
	}
}