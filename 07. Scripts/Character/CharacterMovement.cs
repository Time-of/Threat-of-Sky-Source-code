using UnityEngine;
using UnityEditor;
using Photon.Pun;


public enum EMovementMode : int
{
	WALKING, FALLING
}


/**
 * 작성자: 20181220 이성수
 * 캐릭터의 이동을 담당하기 위해 존재하는 스크립트입니다.
 * ACharacterBase 기반 클래스가 반드시 필요합니다.
 * 물리 기반 움직임을 가집니다.
 * Rigidbody 에서 기본적으로 제공하는 중력은 사용하지 않습니다.
 */
[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviourPun
{
	protected ACharacterBase OwnerCharacter;



	[Header("이동 모드")]

	[ReadOnlyProperty]
	public EMovementMode MovementMode = EMovementMode.WALKING;



	[Header("지상에서 행동")]

	[Tooltip("기본 이동속도입니다.")]
	public float MoveSpeed = 6.0f;

	[Tooltip("달릴 때 이동속도 증가량입니다.")]
	public float SprintSpeedMult = 1.3f;

	public bool bIsSprint = false;



	[Header("점프 및 낙하")]

	[SerializeField, Tooltip("공중에서의 제어력입니다.")]
	private float AirControlMult = 0.75f;

	[ReadOnlyProperty]
	public int CurrentJumpCount = 0;

	public int MaxJumpCount = 1;

	public bool bPressedJump = false;

	public float JumpPower = 15.0f;

	public bool bUseGravity = true;

	public float GravityPower = 30.0f;

	[Tooltip("현재 적용된 낙하 속력이 이 수치보다 낮거나 같은 경우, 착지할 때 OnFallingEnd(true, CurrentGravity)가 호출됩니다.")]
	public float HardFallingGravity = -35.0f;



	[Header("넉백")]

	[SerializeField, ReadOnlyProperty]
	private Vector3 KnockbackVector;

	[SerializeField]
	private float KnockbackResistance = 0.2f;



	[Header("회전")]

	[Tooltip("체크하면 캐릭터가 컨트롤러 방향을 바라봅니다.")]
	public bool bUseControllerRotationYaw = false;

	[Tooltip("체크하면 캐릭터가 회전 방향을 바라봅니다. bUseControllerRotationYaw보다 우선순위가 낮습니다.")]
	public bool bRotateToMoveDirection = true;

	[Tooltip("만약 0 인 경우, 즉시 바라봅니다.")]
	public float RotationSpeed = 5.0f;

	[ReadOnlyProperty]
	public Quaternion TargetRotation = Quaternion.identity;



	[Header("체크용 변수들")]

	[SerializeField]
	private float GroundCheckDistance = 0.01f;

	[SerializeField]
	private float ForwardCheckDistance = 0.1f;

	[SerializeField]
	private LayerMask GroundMask = -1;

	[SerializeField, ReadOnlyProperty]
	private Vector3 GroundNormal;

	[Tooltip("이 각도보다 높은 경사는 오를 수 없습니다.")]
	public float MaxSlopeAngle = 55.0f;

	[SerializeField, ReadOnlyProperty]
	private float CurrentSlopeAngle;

	[SerializeField, ReadOnlyProperty, Tooltip("이동하고자 하는 방향과 지면이 이루는 각도입니다.")]
	private float ForwardSlopeAngle;

	[SerializeField, ReadOnlyProperty, Tooltip("지면으로부터의 거리입니다.")]
	private float DistanceFromGround;



	[Header("현재 상태")]

	[Tooltip("현재 컨트롤할 수 없는 상태인가를 의미하는 변수입니다.")]
	public bool bCannotControlled = false;

	[SerializeField, ReadOnlyProperty]
	private float CurrentGravity;

	[SerializeField, ReadOnlyProperty]
	private bool bIsForwardBlocked = false;

	[ReadOnlyProperty]
	public bool bIsGrounded = false;

	[SerializeField, ReadOnlyProperty]
	private bool bIsJumping = false;

	[ReadOnlyProperty]
	public bool bIsFalling = false;

	[SerializeField, ReadOnlyProperty]
	private bool bIsOnSlope = false;

	[ReadOnlyProperty]
	public Vector3 Velocity;

	[ReadOnlyProperty]
	public Vector3 VelocityXZ;

	[SerializeField, ReadOnlyProperty]
	private Vector3 MovementInputVector;

	public Vector3 GetMovementInput { get => MovementInputVector; }

	[SerializeField, ReadOnlyProperty]
	private Vector3 InAirMovementVector;

	[SerializeField, ReadOnlyProperty]
	private Vector3 FinalMovementVector;

	[SerializeField, ReadOnlyProperty, Tooltip("강제 이동 속도입니다. 조작할 수 없는 상황에만 사용됩니다.")]
	private Vector3 ForceMovementVelocity;

	public Vector3 ForceMovementVector { get => ForceMovementVelocity; set => ForceMovementVelocity = value; }



	[Header("기타")]

	private CapsuleCollider Capsule;

	private Rigidbody RigidComponent;

	private PhysicMaterial BasePhysMat;

	private PhysicMaterial ZeroFricPhysMat;



	void Awake()
	{
		OwnerCharacter = GetComponent<ACharacterBase>();
		Capsule = GetComponentInChildren<CapsuleCollider>();
		RigidComponent = GetComponent<Rigidbody>();

		RigidComponent.velocity = Vector3.zero;
		Velocity = Vector3.zero;
		VelocityXZ = Vector3.zero;

		BasePhysMat = Capsule.material ?? new PhysicMaterial();
		ZeroFricPhysMat = BasePhysMat;
		
		ZeroFricPhysMat.dynamicFriction = 0;
		ZeroFricPhysMat.staticFriction = 0;
	}



	void Start()
	{
		RigidComponent.useGravity = false;
	}



	void FixedUpdate()
	{
		CheckGround();
		CheckForward();

		ApplyGravity();

		CheckJumpInput();

		if (photonView.IsMine)
		{
			UpdateMove();
			UpdateYawRotation();

			MovementInputVector = Vector3.zero;

			if (transform.position.y <= -100.0f)
			{
				GameManager.Instance.FixCharacterPosition(OwnerCharacter);
			}
		}

		VelocityXZ = Velocity;
		VelocityXZ.y = 0;
	}



	#region 사용자 기능 모음
	/// <summary>
	/// 입력을 추가합니다. 물리 기반 움직임이므로, FixedUpdate 주기에 맞춰 호출하는 것을 추천합니다.
	/// </summary>
	/// <param name="Direction">입력의 방향</param>
	/// <param name="Scale">입력의 크기</param>
	public void AddMovementInput(Vector3 Direction, float Scale)
	{
		MovementInputVector += Direction * Scale;

		if (MovementInputVector.sqrMagnitude > 1.0f)
		{
			MovementInputVector = MovementInputVector.normalized;
		}
	}



	public void AddKnockback(Vector3 KnockbackPower)
	{
		KnockbackVector += KnockbackPower;
	}



	public void ResetGravity()
	{
		CurrentGravity = 0.0f;
	}
	#endregion



	#region 움직임에 관한 처리
	// 중력 적용
	void ApplyGravity()
	{
		if (bUseGravity)
		{
			if (bIsGrounded) CurrentGravity = 0.0f;
			else CurrentGravity -= GravityPower * Time.fixedDeltaTime;
		}
	}



	// 움직임
	void UpdateMove()
	{
		if (bCannotControlled)
		{
			FinalMovementVector = Vector3.zero;

			RigidComponent.velocity = ForceMovementVelocity;
			
			return;
		}

		// 공중에서 전방이 막힌 경우, 또는 점프 중 전방이 막힌 경우 속도 초기화
		if ((bIsForwardBlocked && !bIsGrounded) || (bIsJumping && bIsForwardBlocked))
		{
			bIsJumping = false;
			FinalMovementVector = Vector3.zero;
		}

		float FinalSpeed = MoveSpeed *
				(bIsSprint ? SprintSpeedMult : 1.0f) *
				OwnerCharacter.GetStatusComponent()?.GetMoveSpeedMult ?? 1.0f;

		// 지상이거나, 지면에 가까이 있다면
		if (bIsGrounded || (DistanceFromGround <= GroundCheckDistance && !bIsFalling))
		{
			InAirMovementVector = Vector3.zero;

			// 이동 방향과 지면 노말을 외적해서 축을 구하고, 현재 이동 방향을 해당 축으로 경사로의 각도만큼 회전
			FinalMovementVector = Quaternion.AngleAxis(ForwardSlopeAngle, Vector3.Cross(MovementInputVector, GroundNormal)) * MovementInputVector;
		}
		else
		{
			InAirMovementVector = MovementInputVector * AirControlMult;
		}

		Velocity = (FinalMovementVector + InAirMovementVector) * FinalSpeed;

		if (Velocity.sqrMagnitude >= FinalSpeed * FinalSpeed) Velocity = Velocity.normalized * FinalSpeed;

		Debug.DrawLine(transform.position, transform.position + Velocity);

		RigidComponent.velocity = Velocity + transform.up * CurrentGravity + KnockbackVector;

		// 넉백 감소
		if (!KnockbackVector.AlmostEquals(Vector3.zero, 0.1f))
			KnockbackVector = new Vector3(Mathf.Max(KnockbackVector.x - KnockbackResistance, 0.0f),
				Mathf.Max(KnockbackVector.y - KnockbackResistance, 0.0f),
				Mathf.Max(KnockbackVector.z - KnockbackResistance, 0.0f));
		else KnockbackVector = Vector3.zero;
	}



	// Yaw 회전 업데이트
	void UpdateYawRotation()
	{
		// 컨트롤러 회전 사용하는 경우 컨트롤러 회전 그대로 사용
		if (bUseControllerRotationYaw)
		{
			Quaternion LookRotationYaw = TargetRotation;
			LookRotationYaw.x = 0;
			LookRotationYaw.z = 0;

			// RotationSpeed 값이 0 이라면 즉시 바라보기
			if (RotationSpeed == 0) transform.rotation = LookRotationYaw;
			else transform.rotation = Quaternion.Slerp(transform.rotation, LookRotationYaw.normalized,
				RotationSpeed * Time.fixedDeltaTime);
		}
		// 회전 방향 바라보기 설정인 경우
		else if (MovementInputVector != Vector3.zero && bRotateToMoveDirection)
		{
			Quaternion LookRotationYaw = Quaternion.LookRotation(MovementInputVector * 10);
			LookRotationYaw.x = 0;
			LookRotationYaw.z = 0;

			// RotationSpeed 값이 0 이라면 즉시 바라보기
			if (RotationSpeed == 0) transform.rotation = LookRotationYaw;
			else transform.rotation = Quaternion.Slerp(transform.rotation, LookRotationYaw,
				RotationSpeed * Time.fixedDeltaTime);
		}
	}



	void CheckJumpInput()
	{
		if (bPressedJump && CanJump())
		{
			bPressedJump = false;
			bIsJumping = true;
			Jump();
		}
	}



	void Jump()
	{
		if (MovementMode == EMovementMode.FALLING) CurrentJumpCount++;

		CurrentGravity = JumpPower;
	}



	// 점프 가능한지 체크
	public bool CanJump()
	{
		if (MaxJumpCount < 1) return false;

		return CurrentJumpCount < MaxJumpCount;
	}



	void CheckForward()
	{
		bool bCastSuccessed = Physics.CapsuleCast(transform.position + transform.up * (Capsule.height / 2 - Capsule.radius),
			transform.position - transform.up * (Capsule.height / 2 - Capsule.radius),
			Capsule.radius, MovementInputVector, //  + Vector3.down * 0.1f
			out RaycastHit RayHit, ForwardCheckDistance, GroundMask,
			QueryTriggerInteraction.Ignore);

		bIsForwardBlocked = false;

		if (bCastSuccessed)
		{
			bIsForwardBlocked = Vector3.Angle(RayHit.normal, transform.up) > MaxSlopeAngle;
		}
	}



	float GroundCheckModifiedRadius;
	float GroundCheckModifiedDistance;

	void CheckGround()
	{
		GroundNormal = Vector3.up;
		CurrentSlopeAngle = 0.0f;
		ForwardSlopeAngle = 0.0f;
		
		DistanceFromGround = 10000.0f;
		bIsGrounded = false;

		GroundCheckModifiedRadius = Capsule.radius - 0.01f;
		GroundCheckModifiedDistance = (Capsule.height / 2) - Capsule.radius + GroundCheckDistance + 0.01f;

		bool bCastSuccessed = Physics.SphereCast(transform.position, GroundCheckModifiedRadius,
				Vector3.down, out RaycastHit RayHit, GroundCheckModifiedDistance,
				GroundMask, QueryTriggerInteraction.Ignore);

		if (bCastSuccessed)
		{
			GroundNormal = RayHit.normal;

			//CurrentSlopeAngle = 90.0f - Mathf.Asin(GroundNormal.y) * Mathf.Rad2Deg;
			CurrentSlopeAngle = Vector3.Angle(GroundNormal, transform.up);
			ForwardSlopeAngle = Vector3.Angle(GroundNormal, MovementInputVector) - 90.0f;

			bIsOnSlope = CurrentSlopeAngle <= MaxSlopeAngle;

			DistanceFromGround = Mathf.Max(RayHit.distance - Capsule.height / 2, -10.0f);

			bIsGrounded = (DistanceFromGround <= 0.0001f) && bIsOnSlope;
		}

		ChangeMovementMode((bIsGrounded) ? EMovementMode.WALKING : EMovementMode.FALLING);
	}
	#endregion



	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, GroundCheckModifiedRadius);
		Gizmos.DrawWireSphere(transform.position + Vector3.down * GroundCheckModifiedDistance, GroundCheckModifiedRadius);

		if (Capsule)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position + transform.up * (Capsule.height / 2 - Capsule.radius) + (MovementInputVector + Vector3.down * 0.1f) * ForwardCheckDistance,
				Capsule.radius);
			Gizmos.DrawWireSphere(transform.position - transform.up * (Capsule.height / 2 - Capsule.radius) + (MovementInputVector + Vector3.down * 0.1f) * ForwardCheckDistance,
				Capsule.radius);
		}
	}



	#region 상태 변환에 대한 처리
	// MovementMode 변경
	void ChangeMovementMode(EMovementMode MovementModeToChange)
	{
		if (MovementMode == MovementModeToChange) return;

		EMovementMode PrevMovementMode = MovementMode;

		MovementMode = MovementModeToChange;

		OnMovementModeChanged(PrevMovementMode);
	}



	// 간략한 MovementMode 상태 머신 처리
	void OnMovementModeChanged(EMovementMode PrevMovementMode)
	{
		/** 바뀌기 이전 상태에 따른 처리 */
		switch (PrevMovementMode)
		{
			case EMovementMode.WALKING:
				{
					break;
				}

			case EMovementMode.FALLING:
				{
					Capsule.material = BasePhysMat;

					CurrentJumpCount = 0;
					OwnerCharacter.OnFallingEnd(CurrentGravity <= HardFallingGravity, CurrentGravity);
					bIsFalling = false;

					break;
				}

			default:
				break;
		}

		/** 바뀐 후 상태에 따른 처리 */
		switch (MovementMode)
		{
			case EMovementMode.WALKING:
				{
					bIsJumping = false;

					break;
				}

			case EMovementMode.FALLING:
				{
					Capsule.material = ZeroFricPhysMat;

					CurrentJumpCount++;
					OwnerCharacter.OnFalling();
					bIsFalling = true;

					break;
				}

			default:
				break;
		}
	}
	#endregion
}