﻿using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PlayerAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
	public PlayerData playerData;
	public InputActionAsset inputAsset;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
		dstManager.AddComponent<PlayerMoveData>(entity);
		
		dstManager.AddComponent<PlayerChunkCoord>(entity);
		var playerQuery = dstManager.CreateEntityQuery(typeof(PlayerChunkCoord));
		playerQuery.SetSingleton(new PlayerChunkCoord {chunkX = 0, chunkY = 0});

		dstManager.World.GetExistingSystem<PlayerInputSystem>().actionMap = inputAsset.FindActionMap("Player"); 
		dstManager.World.GetExistingSystem<PlayerInputSystem>().playerData = playerData;
		dstManager.World.GetExistingSystem<PlayerMovementSystem>().playerData = playerData;
	}
}