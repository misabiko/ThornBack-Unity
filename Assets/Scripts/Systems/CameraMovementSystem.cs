using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class CameraMovementSystem : SystemBase {
	protected override void OnUpdate() {
		Entities
			.ForEach(
				(ref Rotation rotation, in CameraRotationData rotationData, in RotationEulerXYZ euler)
					=> rotation.Value = quaternion.EulerXYZ(-rotationData.xAngle, 0, 0)
			).ScheduleParallel();
	}
}