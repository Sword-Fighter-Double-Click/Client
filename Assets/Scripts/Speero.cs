using UnityEngine;

// ĳ���� �߻�ȭ Ŭ���� ������� ���Ƿ� ����
public class Speero : Fighter
{
	[Header("Speero Object")]
	[SerializeField] GameObject lethalMoveAnimation;

	// �������� �� �󸶳� �̵��ϴ����� ���ϴ� ����
	[Header("Speero Value")]
	[SerializeField] private float jumpAttackRushPower = 30;
	[SerializeField] private float jumpAttackDownPower = 100;

	private CapsuleCollider2D playerHitBox;

	/// <summary>
	/// ������ �ñر� �ִϸ��̼� ������Ʈ�� �����͸� �����ϴ� ����
	/// </summary>
	private GameObject lethalMoveAnimationClone = null;

	private bool usingLethalMoveAnimation;

	protected override void Start()
	{
		base.Start();

		playerHitBox = GetComponent<CapsuleCollider2D>();
	}

	protected override void Update()
	{
		base.Update();

		// �ñر⸦ ���
		if (usingLethalMoveAnimation)
		{
			// �ִϸ��̼��� ��µǰ� �����Ǿ��ٸ�
			if (lethalMoveAnimationClone == null)
			{
				lethalMoveAnimationClone = null;
				
				usingLethalMoveAnimation = false;
				
				SetPlayerVisible(1);
				
				OffLethalMoveScreen();
				
				// IDLE ���·� �ʱ�ȭ
				fighterAction = FighterAction.None;
			}
		}
	}

	/// <summary>
	/// ĳ���� ��Ʈ�ڽ� Ȱ��ȭ(1), ��Ȱ��ȭ(0)
	/// </summary>
	/// <param name="value"></param>
	void SetPlayerHitBox(int value)
	{
		if (value == 0)
		{
			playerHitBox.enabled = false;
			cantInputTime = float.MaxValue;
		}
		else if (value == 1)
		{
			playerHitBox.enabled = true;
			cantInputTime = 0;
		}
	}

	/// <summary>
	/// ĳ���� �̹��� Ȱ��ȭ(1), ��Ȱ��ȭ(0)
	/// </summary>
	/// <param name="value"></param>
	void SetPlayerVisible(int value)
	{
		if (value == 0)
		{
			spriteRenderer.enabled = false;

		}
		else if (value == 1)
		{
			spriteRenderer.enabled = true;
		}
	}

	/// <summary>
	/// �������� �� ��ġ �̵�
	/// </summary>
	void MoveDuringJumpAttack()
	{
		rigidBody.AddForce(Vector2.down * jumpAttackDownPower + (transform.rotation.y == 0 ? 1 : -1) * jumpAttackRushPower * Vector2.right, ForceMode.Impulse);
	}

	/// <summary>
	/// ī���� ������ ���� ����
	/// </summary>
	/// <param name="value"></param>
	void SetCounterDamageRate(float value)
	{
		counterDamageRate = value;
	}

	/// <summary>
	/// ���� �ñر� �ǰ� ���ο� ���� �ִϸ��̼� ���
	/// </summary>
	void HandleLethalMoveAnimation()
	{
		// �¾Ҵٸ�
		if (hitLethalMove)
		{
			usingLethalMoveAnimation = true;

			// �ִϸ��̼� ������Ʈ ����
			lethalMoveAnimationClone = Instantiate(lethalMoveAnimation);
			
			// ĳ���� �̹��� ��Ȱ��ȭ
			SetPlayerVisible(0);
			
			OnLethalMoveScreen();

			hitLethalMove = false;
		}
		// ���� �ʾҴٸ�
		else
		{
			// IDLE ���·� �ʱ�ȭ
			fighterAction = FighterAction.None;
		}
	}
}
