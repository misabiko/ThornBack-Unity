using Unity.Entities;
using Unity.Mathematics;

public struct PlayerMoveData : IComponentData {
	public float3 moveDirection;
}