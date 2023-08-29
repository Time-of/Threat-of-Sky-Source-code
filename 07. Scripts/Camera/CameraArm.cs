using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 카메라 위치를 최종 조정할 카메라 암 스크립트입니다.
 * 카메라 위치 조정을 돕기 위해 존재합니다.
 */
public class CameraArm : MonoBehaviour
{
	private ACharacterBase OwnerCharacter;



	[Header("Camera Settings")]

	public bool bUseControllerRotation = true;

	public float ArmLength = 5.0f;



	[Header("Camera Collision")]

	public float CameraCollisionRadius = 0.5f;

	[SerializeField]
	private LayerMask CollisionLayer = -1;



	[Header("Camera")]

	[SerializeField, ReadOnlyProperty]
	private Vector3 CameraPosition;

	[SerializeField, ReadOnlyProperty]
	private Camera CameraObject;

	[SerializeField, ReadOnlyProperty]
	private PlayerCamera PlayerCameraComponent;

	public Camera GetCameraObject() { return CameraObject; }



	void Awake()
	{
		OwnerCharacter = GetComponentInParent<ACharacterBase>();
		CameraObject = GetComponentInChildren<Camera>();
		PlayerCameraComponent = GetComponentInChildren<PlayerCamera>();
	}



	void FixedUpdate()
	{
		CalculateCameraPosition();
	}



	void LateUpdate()
	{
		if (OwnerCharacter == null) return;

		if (bUseControllerRotation && OwnerCharacter.GetController() != null)
		{
			transform.rotation = OwnerCharacter.GetControllerRotation();
		}
	}



	void CalculateCameraPosition()
	{
		/*Physics.SphereCast(transform.position, CameraCollisionRadius,
				transform.forward * -1, out RaycastHit RayHit, ArmLength,
				CollisionLayer, QueryTriggerInteraction.Ignore);*/

		Physics.Raycast(transform.position, transform.forward * -1,
				out RaycastHit RayHit, ArmLength + CameraCollisionRadius,
				CollisionLayer, QueryTriggerInteraction.Ignore);

		Vector3 CameraShakePosition = PlayerCameraComponent?.ShakeVector ?? Vector3.zero;

		CameraPosition = (RayHit.point == Vector3.zero) ? transform.position + transform.forward * -ArmLength + CameraShakePosition :
				transform.position + transform.forward * -(RayHit.distance - CameraCollisionRadius) + CameraShakePosition;

		CameraObject.transform.position = CameraPosition;
	}
}
