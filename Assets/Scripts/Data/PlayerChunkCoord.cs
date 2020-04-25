using System;
using Unity.Entities;
using UnityEngine;

public struct PlayerChunkCoord : IComponentData {
	public int chunkX;
	public int chunkY;
}

public struct PlayerTransform : ISharedComponentData, IEquatable<PlayerTransform> {
	public Transform value;
	
	public static implicit operator Transform(PlayerTransform e) => e.value;

	public static implicit operator PlayerTransform(Transform e) => new PlayerTransform { value = e };

	public bool Equals(PlayerTransform other) => Equals(value, other.value);

	public override bool Equals(object obj) => obj is PlayerTransform other && Equals(other);

	public override int GetHashCode() => (value != null ? value.GetHashCode() : 0);
}