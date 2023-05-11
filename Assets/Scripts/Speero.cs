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

	/// <summary>
	/// ������ �ñر� �ִϸ��̼� ������Ʈ�� �����͸� �����ϴ� ����
	/// </summary>
	private GameObject lethalMoveAnimationClone = null;

	private bool usingLethalMoveAnimation;

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
		status.counterDamageRate = value;
	}

	/// <summary>
	/// ���� �ñر� �ǰ� ���ο� ���� �ִϸ��̼� ���
	/// </summary>
	void HandleLethalMoveAnimation()
	{
		// �¾Ҵٸ�
		if (hitUltimate)
		{
			usingLethalMoveAnimation = true;

			// �ִϸ��̼� ������Ʈ ����
			lethalMoveAnimationClone = Instantiate(lethalMoveAnimation);
			
			// ĳ���� �̹��� ��Ȱ��ȭ
			SetPlayerVisible(0);
			
			OnUltimateScreen();

			hitUltimate = false;
		}
		// ���� �ʾҴٸ�
		else
		{
			// IDLE ���·� �ʱ�ȭ
			fighterAction = FighterAction.None;
		}
	}
}
