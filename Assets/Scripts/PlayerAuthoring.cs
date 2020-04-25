using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PlayerAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
	public PlayerData playerData;


	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
		dstManager.SetName(entity, "Player");
		dstManager.AddSharedComponentData(entity, new PlayerTransform{value = transform});
		
		dstManager.AddComponent<PlayerChunkCoord>(entity);
		var playerQuery = dstManager.CreateEntityQuery(typeof(PlayerChunkCoord));
		playerQuery.SetSingleton(new PlayerChunkCoord {chunkX = 0, chunkY = 0});

		dstManager.World.GetExistingSystem<PlayerMovementSystem>().playerData = playerData;
	}
}