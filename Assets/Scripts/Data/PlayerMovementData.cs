using Unity.Entities;
using Unity.Mathematics;

public struct PlayerMoveData : IComponentData {
	public float3 moveDirection;
	public float yAngle;
	public bool jumpInput;
	public bool sprinting;
}