using UnityEngine;

// ĳ���� �߻�ȭ Ŭ���� ������� ��ũ�� ����
public class ArkSha : Fighter
{
	[Header("Arksha Value")]
	// �������� �� �󸶳� �̵��ϴ����� ���ϴ� ����
	[SerializeField] private float jumpAttackUpPower = 5;
	[SerializeField] private float jumpAttackDownPower = 100;
	// �������� �� �ּ��� ���߿� ���ִ� �ð��� ���ϴ� ����
	[SerializeField] private float stayJumpAttack = 0.85f;

	private float originalJumpAttackDamage;

	/// <summary>
	/// �������� ��� ������ Ȯ���ϴ� ����
	/// </summary>
	private bool isStayJumpAttack;

	/// <summary>
	/// �������� �� ���߿� ���ִ� �ð��� �����ϴ� ����
	/// </summary>
	private float countStayJumpAttack;

	private bool isLethalMove;

	protected override void Start()
	{
		base.Start();

		originalJumpAttackDamage = jumpAttackDamage;
	}

	protected override void Update()
	{
		base.Update();

		if (!isStayJumpAttack) return;

		countStayJumpAttack -= Time.deltaTime;

		if (countStayJumpAttack > 0)
		{
			if (Input.GetKeyUp(KeySetting.keys[fighterNumber, 4]))
			{
				TryJumpAttack();
			}
		}
		else
		{
			countStayJumpAttack = 0;
			TryJumpAttack();
		}
	}

	/// <summary>
	/// ���� ���� ���. ���߿� ���ִ� �ð� ī��Ʈ
	/// </summary>
	void StayJumpAttack()
	{
		isStayJumpAttack = true;
		countStayJumpAttack = stayJumpAttack;
	}

	/// <summary>
	/// ���߿� ���ִ� �ð��� ����Ͽ� �������� ������ ����
	/// </summary>
	void SetJumpAttackDamage()
	{
		jumpAttackDamage *= (stayJumpAttack - countStayJumpAttack) / stayJumpAttack;
	}

	/// <summary>
	/// �������� ������ �ʱ�ȭ
	/// </summary>
	void InitializeJumpAttackDamage()
	{
		jumpAttackDamage = originalJumpAttackDamage;
	}

	/// <summary>
	/// �������� ����
	/// </summary>
    private void TryJumpAttack()
	{
		isStayJumpAttack = false;

		animator.CrossFade("TryJumpAttack", 0);

		// �Ʒ��� �̵�
		MoveDuringJumpAttack(-1);
	}

	/// <summary>
	/// ���� �̵�(1), �Ʒ��� �̵�(0)
	/// </summary>
	/// <param name="path"></param>
	void MoveDuringJumpAttack(int path)
	{
		rigidBody.AddForce(path > 0 ? Vector2.up * jumpAttackUpPower : Vector2.down * jumpAttackDownPower, ForceMode.Impulse);
	}

	/// <summary>
	/// �ñر� ȿ�� ���
	/// </summary>
    void HandleLethalMoveAnimation()
    {
		// �¾Ҵٸ�
        if (hitLethalMove)
        {
			// �߰�Ÿ �߻�
			animator.CrossFade("HitLethalMove", 0);

			// �ñر� ���ȭ�� Ȱ��ȭ
            OnLethalMoveScreen();

            hitLethalMove = false;
        }
        else
        {
			// �ñر� ���� �ִϸ��̼� ���
			animator.CrossFade("MissLethalMove", 0);
        }
    }
}