using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class CameraAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
		dstManager.AddComponent<CameraRotationData>(entity);
		dstManager.AddComponent<RotationEulerXYZ>(entity);
	}
}