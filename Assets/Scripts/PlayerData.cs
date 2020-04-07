﻿using UnityEngine;

[CreateAssetMenu]
public class PlayerData : ScriptableObject {
	public float speed = 1f;
	public float sprintSpeed = 2f;
	public float accel = 100f;
	public float jumpForce = 3f;
	public float deccel = 0.1f;

	public float camSensitivityX = 1f;
	public float camSensitivityY = 1f;
	public float tweakSensitivity = 0.004f;
}