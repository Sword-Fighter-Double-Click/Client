using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class Fighter : MonoBehaviour
{
    public enum FighterAction
    {
        None = 0,
        Hit = 1,
        Jump = 2,
        Guard = 3,
        Attack = 4,
        ChargedAttack = 5,
        JumpAttack = 6,
        LethalMove = 7
    };

    [Header("Caching")]
	[SerializeField] private Image HPBar;
	[SerializeField] private Image FPBar;
	[SerializeField] private GameObject lethalMoveScreen;
	[SerializeField] private AnimationClip lethalMove;
	[SerializeField] private BoxCollider2D[] motionColliders = new BoxCollider2D[4];

	// ���� ���¿� �ִ� ���� ȿ�� ������ ������Ʈ
	[SerializeField] private GameObject m_RunStopDust;
	[SerializeField] private GameObject m_JumpDust;
	[SerializeField] private GameObject m_LandingDust;

	[Header("Action")]
	// Attack, None, Guard�� ���� �÷��̾��� ���¸� �����ϴ� ����
	public FighterAction fighterAction;

	[Header("Value")]
	// 0���� ���� �÷��̾�, 1���� ������ �÷��̾�� ������
	[Range(0, 1)] public int fighterNumber = 0;
	/// <summary>
	/// Ű �Է� ���θ� ���ϴ� ����
	/// </summary>
	public bool canInput = true;
	// ��� ĳ���� Ŭ���� ���� ����
	public Fighter enemyFighter;

	// �ִ� �ӵ�, ���� ����, ī���� ������ ����, ���� ������, ���� �� ������ ������ ��� ���� �������ͽ� ���� �����ϴ� ������
	[Header("Stats")]
	public float HP = 100;
	public float FP = 0;
	[SerializeField] protected float maxSpeed = 4.5f;
	[SerializeField] protected float jumpForce = 7.5f;
	[SerializeField] protected float counterDamageRate = 1.2f;
	[SerializeField] protected float attackDamage = 6f;
	[SerializeField] protected float chargedAttackDamage = 12;
	[SerializeField] protected float jumpAttackDamage = 15;
	[SerializeField] protected float lethalMoveDamage = 35;
	[SerializeField] protected float attackAbsorptionRate = 0.95f;
	[SerializeField] protected float chargedAttackAbsorptionRate = 0.75f;
	[SerializeField] protected float jumpAttackAbsorptionRate = 0.7f;
	[SerializeField] protected float lethalMoveAbsorptionRate = 0.5f;
	[SerializeField] protected float chargedAttackFP = 3;
	[SerializeField] protected float jumpAttackCoolFP = 5;
	[SerializeField] protected float lethalMoveFP = 75;
	[SerializeField] protected float chargedAttackCoolDownTime = 1.2f;
	[SerializeField] protected float jumpAttackCoolDownTime = 1.5f;
	[SerializeField] protected float hitKnockBackPower = 10;
	[SerializeField] protected float guardKnockBackPower = 5;

	protected Animator animator;
	protected Rigidbody rigidBody;
	protected SpriteRenderer spriteRenderer;
	private Sensor groundSensor;
	//private FighterAudio fighterAudio;

	/// <summary>
	/// �׶��� üũ ����
	/// </summary>
	protected bool isGround = false;
	/// <summary>
	/// �̵� ���� ���� -1�� ��, 1�� ��
	/// </summary>
	private int facingDirection = 0;

	// �����ݰ� ���������� ��Ÿ���� ����ϴ� ����
	private float countChargedAttack;
	private float countJumpAttack;

	// Ű �Է��� �� ���� �ð��� ���ϴ� ����
	protected float cantInputTime = 0;

	/// <summary>
	/// ���� �ñر⿡ �¾Ҵ����� �����ϴ� ����
	/// </summary>
	protected bool hitLethalMove;

	private GameObject lethalMoveScreenClone;

	private void Awake()
	{
		animator = GetComponentInChildren<Animator>();
		rigidBody = GetComponent<Rigidbody>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		//fighterAudio = transform.Find("Audio").GetComponent<FighterAudio>();
		groundSensor = GetComponentInChildren<Sensor>();
	}

	// �ڽ� Ŭ���������� �� �� �ֵ��� �߻�ȭ
	protected virtual void Start()
	{
		SettingUI();
	}

	// �ڽ� Ŭ���������� �� �� �ֵ��� �߻�ȭ
	protected virtual void Update()
	{
		HandleCantInputTime(Time.deltaTime);

		Death();

		OnGround();

		SetAirspeed();

		CountCoolTime(Time.deltaTime);

		HandleUI();

		Jump();

		Movement();

		Guard();

		Attack();

		ChargedAttack();

		JumpAttack();

		LethalMove();

		// ���� �ִ� Collider�� ũ�⸦ �̿��Ͽ� ���������� �����ϰ� ���� �¾Ҵ��� Ȯ���ϴ� �ݺ���
		foreach (BoxCollider2D collider in motionColliders)
		{
			if (!collider.enabled) continue;

			if (!SearchFighterWithinRange(collider)) continue;

			// ���� �¾Ҵٸ�

			if (fighterAction == FighterAction.LethalMove)
			{
				hitLethalMove = true;
			}

			collider.enabled = false;

			// �������� ����
			GiveDamage(enemyFighter);
		}
	}

	#region HandleValue

	/// <summary>
	/// ���� fighterNumber�� ���� UI ĳ��
	/// </summary>
	private void SettingUI()
	{
		if (fighterNumber == 0)
		{
			GameObject player1UI = GameObject.FindGameObjectWithTag("Player1UI");
			HPBar = player1UI.transform.Find("HPBar").GetComponent<Image>();
			FPBar = player1UI.transform.Find("FPBar").GetComponent<Image>();
			//rigidBody.sharedMaterial = GameObject.Find("Fighter1PhysicsMaterial").GetComponent<Rigidbody2D>().sharedMaterial;
		}
		else if (fighterNumber == 1)
		{
			GameObject player2UI = GameObject.FindGameObjectWithTag("Player2UI");
			HPBar = player2UI.transform.Find("HPBar").GetComponent<Image>();
			FPBar = player2UI.transform.Find("FPBar").GetComponent<Image>();
			//rigidBody.sharedMaterial = GameObject.Find("Fighter2PhysicsMaterial").GetComponent<Rigidbody2D>().sharedMaterial;
		}
	}

	/// <summary>
	/// fighterNumber�� ���� �±� ���� �� �������ͽ�, �ִϸ��̼�, �������� �ʱ�ȭ
	/// </summary>
	public void ResetState()
	{
		// �±� ����
		if (fighterNumber == 0)
		{
			tag = "Player1";
			//spriteRenderer.flipX = false;
		}
		else if (fighterNumber == 1)
		{
			tag = "Player2";
			//spriteRenderer.flipX = true;
		}

		// �ִϸ��̼� �� �������ͽ� �ʱ�ȭ
		animator.SetInteger("FacingDirection", 0);
		HP = 100;
		FP = 0;
		SetAction(0);
		animator.SetTrigger("RoundStart");

		// �������� �ʱ�ȭ
		foreach (BoxCollider2D boxCollider2D in motionColliders)
		{
			boxCollider2D.enabled = false;
		}
	}

	/// <summary>
	/// �׶��� üũ
	/// </summary>
	private void OnGround()
	{
		if (fighterAction == FighterAction.JumpAttack) return;

		if (!isGround && groundSensor.State())
		{
			// ������ 10���� ����
			//rigidBody.sharedMaterial.friction = 10;
			isGround = true;
			fighterAction = FighterAction.None;
			animator.SetBool("Grounded", isGround);
			animator.SetBool("Jump", false);
		}
	}

	/// <summary>
	/// AirSpeed �� �Ҵ�
	/// </summary>
	private void SetAirspeed()
	{
		// airSpeedY�� 0 ���ϰ� �Ǹ� ���� �ִϸ��̼� ���
		animator.SetFloat("AirSpeedY", rigidBody.velocity.y);
	}

	/// <summary>
	/// �����ݰ� �������� ��Ÿ�� ���
	/// </summary>
	/// <param name="deltaTime"></param>
	private void CountCoolTime(float deltaTime)
	{
		if (countChargedAttack > 0)
		{
			countChargedAttack -= deltaTime;
		}
		if (countJumpAttack > 0)
		{
			countJumpAttack -= deltaTime;
		}
	}

	/// <summary>
	/// �÷��̾��� ���¸� ������ ����
	/// </summary>
	/// <param name="value"></param>
	void SetAction(int value)
	{
		if (value < (int)FighterAction.None && value > (int)FighterAction.LethalMove) return;

		fighterAction = (FighterAction)value;
	}

	/// <summary>
	/// �Է� �Ұ� �ð� ���
	/// </summary>
	/// <param name="deltaTime"></param>
	private void HandleCantInputTime(float deltaTime)
	{
		// �Է� �Ұ� �ð��� ���������� Ű �Է� �Ұ�
		if (cantInputTime > 0)
		{
			cantInputTime -= deltaTime;

			canInput = false;
		}
		else
		{
			canInput = true;
		}
	}

	/// <summary>
	/// �Է� �Ұ� �ð� ����
	/// </summary>
	/// <param name="value"></param>
	void SetCantInputTime(float value)
	{
		cantInputTime = value;
	}

	/// <summary>
	/// �Է��� �����ϰ� ����
	/// </summary>
	public void OnInput()
	{
		cantInputTime = 0;
	}

	/// <summary>
	/// �Է��� �Ұ����ϰ� ����
	/// </summary>
	public void OffInput()
	{
		cantInputTime = float.MaxValue;
	}
	#endregion

	#region HandleHitBox
	/// <summary>
	/// ���� �� �������� �ش��ϴ� ���� ���� collider Ȱ��ȭ
	/// </summary>
	/// <param name="number"></param>
	void OnHitBox(int number)
	{
		motionColliders[number].enabled = true;
	}

	/// <summary>
	/// ���� �� �������� �ش��ϴ� ���� ���� collider ��Ȱ��ȭ
	/// </summary>
	/// <param name="number"></param>
	void OffHitBox(int number)
	{
		motionColliders[number].enabled = false;
	}
	#endregion

	#region HandleUI
	/// <summary>
	/// ü�� �� ��ų ������ UI�� ǥ��
	/// </summary>
	private void HandleUI()
	{
		HPBar.fillAmount = HP * 0.01f;
		FPBar.fillAmount = FP * 0.01f;
	}
	#endregion

	#region Action
	/// <summary>
	/// �̵� �� �÷��̾� ���� ����
	/// </summary>
	private void Movement()
	{
		if (!canInput) return;
		// IDLE�� ������ ���� �̵� �Լ� ����
		if (!(fighterAction == FighterAction.None || fighterAction == FighterAction.Jump)) return;

		// Ű �Է� ���� ����
		bool inputRight = Input.GetKey(KeySetting.keys[fighterNumber, 3]);
		bool inputLeft = Input.GetKey(KeySetting.keys[fighterNumber, 1]);

		// ���� ���� �� ����
		int direction = (inputLeft ? -1 : 0) + (inputRight ? 1 : 0);
		facingDirection = direction;

		// �̵� �ִϸ��̼� ���
		animator.SetInteger("FacingDirection", facingDirection);

		// �̵� ���⿡ ���� �̹��� ���� ����
		transform.eulerAngles = (enemyFighter.transform.position.x > transform.position.x ? Vector3.zero : Vector3.up * 180);
		
		// �̵�
		rigidBody.velocity = new Vector2(facingDirection * maxSpeed, rigidBody.velocity.y);
	}

	/// <summary>
	/// ����
	/// </summary>
	private void Jump()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE ���¿��� �Լ� ����
		if (fighterAction != FighterAction.None) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 0]))
		{
			// ������ ������ ���� ������ 0���� ����
			//rigidBody.sharedMaterial.friction = 0;
			isGround = false;
			fighterAction = FighterAction.Jump;
			animator.SetBool("Grounded", isGround);
			// �ִϸ��̼� ���
			animator.SetBool("Jump", true);
			// ����
			rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
			groundSensor.Disable(0.2f);
		}
	}
	
	/// <summary>
	/// ����
	/// </summary>
	private void Guard()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE �� ���� ���¿��� �Լ� ����
		if (!(fighterAction == FighterAction.None || fighterAction == FighterAction.Guard)) return;

		// Ű�� ������ ������ ���� Ȱ��ȭ, ���� ���� ��Ȱ��ȭ
		if (Input.GetKey(KeySetting.keys[fighterNumber, 2]))
		{
			fighterAction = FighterAction.Guard;

			animator.CrossFade("Guard", 0f);
		}
		else if (Input.GetKeyUp(KeySetting.keys[fighterNumber, 2]))
		{
			fighterAction = FighterAction.None;
			animator.SetTrigger("UnGuard");
		}
	}

	/// <summary>
	/// �Ϲݰ���
	/// </summary>
	private void Attack()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE ���¿��� �Լ� ����
		if (fighterAction != FighterAction.None) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
		{
			fighterAction = FighterAction.Attack;

			animator.CrossFade("Attack", 0);
		}
	}

	/// <summary>
	/// ������
	/// </summary>
	private void ChargedAttack()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE ���¿��� �Լ� ����
		if (fighterAction != FighterAction.None) return;
		if (countChargedAttack > 0) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 5]))
		{
			fighterAction = FighterAction.ChargedAttack;

			animator.CrossFade("ChargedAttack", 0);
			
			// ������ ���� ��Ÿ�� ����
			countChargedAttack = chargedAttackCoolDownTime;
		}
	}

	/// <summary>
	/// ��������
	/// </summary>
	private void JumpAttack()
	{
		if (!canInput) return;
		if (isGround) return;
		// ���� ���¿����� �Լ� ����
		if (fighterAction != FighterAction.Jump) return;
		if (countJumpAttack > 0) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 4]))
		{
			fighterAction = FighterAction.JumpAttack;

			animator.CrossFade("JumpAttack", 0);
			
			// �������� ���� ��Ÿ�� ����
			countJumpAttack = jumpAttackCoolDownTime;
		}
	}

	/// <summary>
	/// �ñر�
	/// </summary>
	private void LethalMove()
	{
		if (!canInput) return;
		if (!isGround) return;
		// IDLE ���¿����� �Լ� ����
		if (fighterAction != FighterAction.None) return;

		if (Input.GetKeyDown(KeySetting.keys[fighterNumber, 6]))
		{
			if (FP < 100) return;
			
			FP = 0;
			
			fighterAction = FighterAction.LethalMove;

			animator.CrossFade("LethalMove", 0);
		}
	}

	/// <summary>
	/// �ǰ� 
	/// </summary>
	/// <param name="isGuard"></param>
	/// <param name="enemyRotationY"></param>
	/// <param name="lethalMoveCantInputTime"></param>
	private void Hit(bool isGuard, float enemyRotationY, float lethalMoveCantInputTime)
	{
		Vector2 knockBackPath = enemyRotationY == 0 ? Vector2.right : Vector2.left;

		// ���� �� �Է� �Ұ��� �˹��� �ð��� ���带 ������ ������ �پ��ϴ�.
		// �ñر�� �����ð��� ���� ������ ���������� �Է��� ���ϰ� ����ϴ�.
		if (isGuard)
		{
			// �Է� �Ұ� �ð� ����
			cantInputTime = lethalMoveCantInputTime > 0 ? lethalMoveCantInputTime : 0.1f;
			// �˹�
			rigidBody.AddForce(knockBackPath * guardKnockBackPower, ForceMode.Impulse);
		}
		else
		{
			// ���� ���� �ߴ�
			for (int count = 0; count < motionColliders.Length; count++)
			{
				OffHitBox(count);
			}
			SetAction(0);
			// �Է� �Ұ� �ð� ����
			cantInputTime = lethalMoveCantInputTime > 0 ? lethalMoveCantInputTime : 0.3f;
			// �˹�
			rigidBody.AddForce(knockBackPath * hitKnockBackPower, ForceMode.Impulse);
		}
	}

	/// <summary>
	/// ���
	/// </summary>
	private void Death()
	{
		// HP�� 0�� �Ǹ� �ִϸ��̼��� ���
		if (HP <= 0)
		{
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
			{
				animator.CrossFade("Death", 0);
				OffInput();
			}
		}
	}
	#endregion

	#region Action Effect
	// ���� ���� ȿ�� �Լ�
	private void SpawnDustEffect(GameObject dust, float dustXOffset = 0)
	{
		if (dust != null)
		{
			Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * facingDirection, 0f, 0f);
			GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity);
			newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(facingDirection, 1, 1);
		}
	}

	//void AE_runStop()
	//{
	//	fighterAudio.PlaySound("RunStop");
	//	float dustXOffset = 0.6f;
	//	SpawnDustEffect(m_RunStopDust, dustXOffset);
	//}

	//void AE_footstep()
	//{
	//	fighterAudio.PlaySound("Footstep");
	//}

	//void AE_Jump()
	//{
	//	fighterAudio.PlaySound("Jump");
	//	SpawnDustEffect(m_JumpDust);
	//}

	//void AE_Landing()
	//{
	//	fighterAudio.PlaySound("Landing");
	//	SpawnDustEffect(m_LandingDust);
	//}

	/// <summary>
	/// �ñر� �̹��� Ȱ��ȭ
	/// </summary>
	protected void OnLethalMoveScreen()
	{
		lethalMoveScreenClone = Instantiate(lethalMoveScreen);

		lethalMoveScreenClone.SetActive(true);
	}

	/// <summary>
	/// �ñر� �̹��� ��Ȱ��ȭ
	/// </summary>
	protected void OffLethalMoveScreen()
	{
		Destroy(lethalMoveScreenClone);
	}
	#endregion

	#region HandleHitDetection
	/// <summary>
	/// �������� ���� �� ���������� �νĵ� �� �÷��̾� ������ ��ȯ
	/// </summary>
	/// <param name="searchRange"></param>
	/// <returns></returns>
	private bool SearchFighterWithinRange(Collider2D searchRange)
	{
		// BoxCast�� �������� ����
		RaycastHit2D[] raycastHits = Physics2D.BoxCastAll(searchRange.bounds.center, searchRange.bounds.size, 0f, transform.rotation.y == 0 ? Vector2.right : Vector2.left, 0.01f, LayerMask.GetMask("Player"));

		foreach (RaycastHit2D raycastHit in raycastHits)
		{
			if (raycastHit.collider.GetComponent<Fighter>() == enemyFighter)
			{
				return true;
			}
		}

		// ã�� ���ϸ� null ��ȯ
		return false;
	}

	/// <summary>
	/// �� �÷��̾�� �������� ���ϴ� �Լ�
	/// </summary>
	/// <param name="enemyFighter"></param>
	private void GiveDamage(Fighter enemyFighter)
	{
		bool isGuard = false;

		// ���� �� ������ �迭�� ����
		float[] damages = { attackDamage, chargedAttackDamage, jumpAttackDamage, lethalMoveDamage };

		// ���� �� ���� �� ������ ������ �迭�� ����
		float[] absorptionRates = { attackAbsorptionRate, chargedAttackAbsorptionRate, jumpAttackAbsorptionRate, lethalMoveAbsorptionRate };

		float damage = 0;

		// �ǰ� ���� �Է� �Ұ� �ð��� �ִٸ� �Ʒ� �ڵ带 �������� ����
		if (cantInputTime > 0) return;

		// ���� ������ ������ ����
		damage += damages[(int)fighterAction - 4];

		// ���� �� ������ ����
		if (enemyFighter.fighterAction == FighterAction.Guard)
		{
			damage = damages[(int)fighterAction - 4] - damages[(int)fighterAction - 4] * absorptionRates[(int)fighterAction - 4];

			isGuard = true;

			cantInputTime = 0.5f;

			SetAction(0);
		}
		// �� �÷��̾ ���� �ִϸ��̼��� ��, ī� ������ ���Ͽ� ������ ����
		else if (!(enemyFighter.fighterAction == FighterAction.None || enemyFighter.fighterAction == FighterAction.Hit))
		{
			damage *= counterDamageRate;
		}

		// ���� ������ �ñر��� ��, �����ð�+0.5���� �Է� �Ұ� �ð��� ������ �����Ѵ�.
		float lethalMoveCantInputTime = 0;
		if (fighterAction == FighterAction.LethalMove)
		{
			lethalMoveCantInputTime = lethalMove.length + 0.5f;
		}

		enemyFighter.HP -= damage;
		enemyFighter.Hit(isGuard, transform.rotation.y, lethalMoveCantInputTime);

		FP = Mathf.Clamp(FP + 10, 0, 100);
		enemyFighter.FP = Mathf.Clamp(enemyFighter.FP + 5, 0, 100);
	}
	#endregion
}
