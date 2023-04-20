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
	[SerializeField] private float maxStayJumpAttack = 1.5f;

	private float originalJumpAttackDamage;

	/// <summary>
	/// �������� ��� ������ Ȯ���ϴ� ����
	/// </summary>
	private bool isStayJumpAttack;

	/// <summary>
	/// �������� �� ���߿� ���ִ� �ð��� �����ϴ� ����
	/// </summary>
	private float countStayJumpAttack;

	private bool pressJumpAttack;

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

		countStayJumpAttack += Time.deltaTime;

		pressJumpAttack = Input.GetKey(KeySetting.keys[fighterNumber, 4]);

		if ((pressJumpAttack ? maxStayJumpAttack : stayJumpAttack) - countStayJumpAttack <= 0 || isGround)
		{
			TryJumpAttack();
		}
	}

	/// <summary>
	/// ���� ���� ���. ���߿� ���ִ� �ð� ī��Ʈ
	/// </summary>
	void StayJumpAttack()
	{
		isStayJumpAttack = true;
		pressJumpAttack = false;
		countStayJumpAttack = 0;
	}

	/// <summary>
	/// ���߿� ���ִ� �ð��� ����Ͽ� �������� ������ ����
	/// </summary>
	void SetJumpAttackDamage()
	{
		jumpAttackDamage *= Mathf.Clamp(countStayJumpAttack, 0.3f, 1) / maxStayJumpAttack;
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
		rigidBody.AddForce(path > 0 ? Vector3.up * jumpAttackUpPower : Vector3.down * jumpAttackDownPower, ForceMode.Impulse);
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