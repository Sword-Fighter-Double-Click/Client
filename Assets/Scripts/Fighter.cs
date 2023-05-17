using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Fighter : MonoBehaviour
{
    public enum FighterAction
    {
        None,
        Hit,
        Jump,
        Guard,
        Attack,
        ChargedAttack,
        JumpAttack,
        Ultimate
    };

    public enum FighterPosition
    {
        Left,
        Right
    }

    [Header("Caching")]
    [SerializeField] private Image HPBar;
    [SerializeField] private Image FPBar;
    [SerializeField] private GameObject ultimateScreen;
    [SerializeField] private AnimationClip ultimate;

    // ���� ���¿� �ִ� ���� ȿ�� ������ ������Ʈ
    //[SerializeField] private GameObject m_RunStopDust;
    //[SerializeField] private GameObject m_JumpDust;
    //[SerializeField] private GameObject m_LandingDust;

    [Header("Action")]
    // Attack, None, Guard�� ���� �÷��̾��� ���¸� �����ϴ� ����
    public FighterAction fighterAction;

    [Header("Value")]
    // 0���� ���� �÷��̾�, 1���� ������ �÷��̾�� ������
    protected FighterPosition fighterPosition;
    /// <summary>
    /// Ű �Է� ���θ� ���ϴ� ����
    /// </summary>
    [SerializeField] private bool canInput = true;
    // ��� ĳ���� Ŭ���� ���� ����
    private Fighter enemyFighter;

    // �ִ� �ӵ�, ���� ����, ī���� ������ ����, ���� ������, ���� �� ������ ������ ��� ���� �������ͽ� ���� �����ϴ� ������
    [Header("Stats")]
    public float currentHP;
    public float currentUltimateGage;
    [SerializeField] protected FighterStatus status;
    [SerializeField] protected FighterSkill[] skills = new FighterSkill[5];

    protected Animator animator;
    protected Rigidbody rigidBody;
    protected SpriteRenderer spriteRenderer;
    private Sensor groundSensor;
    protected AudioSource audioSource;
    //private FighterAudio fighterAudio;

    /// <summary>
    /// �׶��� üũ ����
    /// </summary>
    protected bool isGround = false;
    /// <summary>
    /// �̵� ���� ���� -1�� ��, 1�� ��
    /// </summary>
    private int facingDirection = 0;

    // Ű �Է��� �� ���� �ð��� ���ϴ� ����
    protected float cantInputTime = 0;

    /// <summary>
    /// ���� �ñر⿡ �¾Ҵ����� �����ϴ� ����
    /// </summary>
    protected bool hitUltimate;

    private GameObject ultimateScreenClone;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //fighterAudio = transform.Find("Audio").GetComponent<FighterAudio>();
        groundSensor = GetComponentInChildren<Sensor>();
        audioSource = Camera.main.GetComponent<AudioSource>();
    }

    // �ڽ� Ŭ���������� �� �� �ֵ��� �߻�ȭ
    protected virtual void Update()
    {
        HandleCantInputTime(Time.deltaTime);

        Death();

        OnGround();

        SetAirspeed();

        HandleUI();

        Jump();

        Movement();

        Guard();

        Attack();

        ChargedAttack();

        JumpAttack();

        Ultimate();

        // ���� �ִ� Collider�� ũ�⸦ �̿��Ͽ� ���������� �����ϰ� ���� �¾Ҵ��� Ȯ���ϴ� �ݺ���
        foreach (FighterSkill skill in skills)
        {
            if (!skill.colliderEnabled) continue;

            if (SearchFighterWithinRange(skill.collider))
            {
                // ���� �¾Ҵٸ�

                if (fighterAction == FighterAction.Ultimate)
                {
                    hitUltimate = true;
                }

                skill.colliderEnabled = false;

                // �������� ����
                GiveDamage(skill);
            }
        }
    }

    #region HandleValue

    /// <summary>
    /// ���� fighterNumber�� ���� UI ĳ��
    /// </summary>
    public void SettingUI()
    {
        Transform UI;
        foreach (string word in new string[] { "1", "2" })
        {
            string temp = "Player" + word;
            if (CompareTag(temp))
            {
                UI = GameObject.FindGameObjectWithTag(temp + "UI").transform;
                HPBar = UI.Find("HPBar").GetComponent<Image>();
                FPBar = UI.Find("Ultimate").Find("Empty").GetComponent<Image>();
            }
        }
    }

    /// <summary>
    /// fighterNumber�� ���� �±� ���� �� �������ͽ�, �ִϸ��̼�, �������� �ʱ�ȭ
    /// </summary>
    public void ResetState()
    {
        // �ִϸ��̼� �� �������ͽ� �ʱ�ȭ
        animator.SetInteger("FacingDirection", 0);
        currentHP = status.HP;
        currentUltimateGage = 0;
        SetAction(FighterAction.None);
        animator.SetTrigger("Reset");

        if (CompareTag("Player1"))
        {
            fighterPosition = FighterPosition.Left;
        }
        else if(CompareTag("Player2"))
        {
            fighterPosition = FighterPosition.Right;
        }

        // �������� �ʱ�ȭ
        foreach (FighterSkill skill in skills)
        {
            skill.colliderEnabled = false;
        }

        // rigidbody �ʱ�ȭ
        rigidBody.velocity = Vector3.zero;
    }

    public void SetEnemyFighter(Fighter fighter)
    {
        enemyFighter = fighter;
    }

    /// <summary>
    /// �׶��� üũ
    /// </summary>
    private void OnGround()
    {
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
    /// �÷��̾��� ���¸� ������ ����
    /// </summary>
    /// <param name="value"></param>
    void SetAction(FighterAction value)
    {
        //if (value < (int)FighterAction.None && value > (int)FighterAction.Ultimate) return;

        fighterAction = value;
    }

    void SetAction(string value)
    {
        fighterAction = (FighterAction)Enum.Parse(typeof(FighterAction), value);
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
    /// ���� �� ���ڿ��� �ش��ϴ� ���� ���� collider Ȱ��ȭ �� ��Ȱ��ȭ ����
    /// </summary>
    /// <param name="action"></param>
    /// <param name="enable"></param>
    void OnHitBox(FighterAction action)
    {
        skills[(int)action].colliderEnabled = true;
    }

    void OffHitBox(FighterAction action)
    {
        skills[(int)action].colliderEnabled = false;
    }

    void OnHitBox(string value)
    {
        int string2Number = (int)(FighterAction)Enum.Parse(typeof(FighterAction), value);
        skills[string2Number].colliderEnabled = true;
    }

    void OffHitBox(string value)
    {
        int string2Number = (int)(FighterAction)Enum.Parse(typeof(FighterAction), value);
        skills[string2Number].colliderEnabled = false;
    }
    #endregion

    #region HandleUI
    /// <summary>
    /// ü�� �� ��ų ������ UI�� ǥ��
    /// </summary>
    private void HandleUI()
    {
        HPBar.fillAmount = currentHP / status.HP;
        FPBar.fillAmount = currentUltimateGage / status.ultimateGage;
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
        bool inputRight = Input.GetKey(KeySetting.keys[(int)fighterPosition, 3]);
        bool inputLeft = Input.GetKey(KeySetting.keys[(int)fighterPosition, 1]);

        // ���� ���� �� ����
        int direction = (inputLeft ? -1 : 0) + (inputRight ? 1 : 0);
        facingDirection = direction;

        // �̵� �ִϸ��̼� ���
        animator.SetInteger("FacingDirection", facingDirection);

        // �̵� ���⿡ ���� �̹��� ���� ����
        transform.eulerAngles = (enemyFighter.transform.position.x > transform.position.x ? Vector3.zero : Vector3.up * 180);

        // �̵�
        rigidBody.velocity = new Vector3(facingDirection * status.speed, rigidBody.velocity.y);
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

        if (Input.GetKeyDown(KeySetting.keys[(int)fighterPosition, 0]))
        {
            // ������ ������ ���� ������ 0���� ����
            //rigidBody.sharedMaterial.friction = 0;
            isGround = false;
            fighterAction = FighterAction.Jump;
            animator.SetBool("Grounded", isGround);
            // �ִϸ��̼� ���
            animator.SetBool("Jump", true);
            // ����
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, status.jumpForce);
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
        if (Input.GetKey(KeySetting.keys[(int)fighterPosition, 2]))
        {
            fighterAction = FighterAction.Guard;

            animator.CrossFade("Guard", 0f);
        }
        else if (Input.GetKeyUp(KeySetting.keys[(int)fighterPosition, 2]))
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

        if (Input.GetKeyDown(KeySetting.keys[(int)fighterPosition, 4]))
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

        if (Input.GetKeyDown(KeySetting.keys[(int)fighterPosition, 5]))
        {
            fighterAction = FighterAction.ChargedAttack;

            animator.CrossFade("ChargedAttack", 0);
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

        if (Input.GetKeyDown(KeySetting.keys[(int)fighterPosition, 4]))
        {
            fighterAction = FighterAction.JumpAttack;

            animator.CrossFade("JumpAttack", 0);
        }
    }

    /// <summary>
    /// �ñر�
    /// </summary>
    private void Ultimate()
    {
        if (!canInput) return;
        if (!isGround) return;
        // IDLE ���¿����� �Լ� ����
        if (fighterAction != FighterAction.None) return;

        if (Input.GetKeyDown(KeySetting.keys[(int)fighterPosition, 6]))
        {
            if (currentUltimateGage < status.ultimateGage) return;

            currentUltimateGage = 0;

            fighterAction = FighterAction.Ultimate;

            animator.CrossFade("Ultimate", 0);
        }
    }

    /// <summary>
    /// �ǰ� 
    /// </summary>
    /// <param name="isGuard"></param>
    /// <param name="enemyRotationY"></param>
    /// <param name="ultimateCantInputTime"></param>
    private void Hit(bool isGuard, float enemyRotationY, float ultimateCantInputTime)
    {
        Vector2 knockBackPath = enemyRotationY == 0 ? Vector2.right : Vector2.left;

        // ���� �� �Է� �Ұ��� �˹��� �ð��� ���带 ������ ������ �پ��ϴ�.
        // �ñر�� �����ð��� ���� ������ ���������� �Է��� ���ϰ� ����ϴ�.
        if (isGuard)
        {
            // �Է� �Ұ� �ð� ����
            cantInputTime = ultimateCantInputTime > 0 ? ultimateCantInputTime : 0.1f;
            // �˹�
            rigidBody.AddForce(knockBackPath * status.guardKnockBackPower, ForceMode.Impulse);
        }
        else
        {
            // ���� ���� �ߴ�
            OffHitBox(fighterAction);
            animator.CrossFade("Hit", 0);
            SetAction(FighterAction.Hit);
            // �Է� �Ұ� �ð� ����
            cantInputTime = ultimateCantInputTime > 0 ? ultimateCantInputTime : 0.3f;
            // �˹�
            rigidBody.AddForce(knockBackPath * status.hitKnockBackPower, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// ���
    /// </summary>
    private void Death()
    {
        // HP�� 0�� �Ǹ� �ִϸ��̼��� ���
        if (currentHP <= 0)
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
    //private void SpawnDustEffect(GameObject dust, float dustXOffset = 0)
    //{
    //    if (dust != null)
    //    {
    //        Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * facingDirection, 0f, 0f);
    //        GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity);
    //        newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(facingDirection, 1, 1);
    //    }
    //}

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
    protected void OnUltimateScreen()
    {
        //ultimateScreenClone = Instantiate(ultimateScreen);

        //ultimateScreenClone.SetActive(true);
    }

    /// <summary>
    /// �ñر� �̹��� ��Ȱ��ȭ
    /// </summary>
    protected void OffUltimateScreen()
    {
        //Destroy(ultimateScreenClone);
    }
    #endregion

    #region HandleHitDetection
    /// <summary>
    /// �������� ���� �� ���������� �νĵ� �� �÷��̾� ������ ��ȯ
    /// </summary>
    /// <param name="searchRange"></param>
    /// <returns></returns>
    private bool SearchFighterWithinRange(BoxCollider searchRange)
    {
        int count = Physics.OverlapBoxNonAlloc(searchRange.bounds.center, searchRange.bounds.size /2, new Collider[2],Quaternion.identity,LayerMask.GetMask("Player"));

        return count > 1;
    }

    /// <summary>
    /// �� �÷��̾�� �������� ���ϴ� �Լ�
    /// </summary>
    /// <param name="enemyFighter"></param>
    private void GiveDamage(FighterSkill fighterSkill)
    {
        bool isGuard = false;

        float damage = 0;

        // �ǰ� ���� �Է� �Ұ� �ð��� �ִٸ� �Ʒ� �ڵ带 �������� ����
        if (cantInputTime > 0) return;

        // ���� ������ ������ ����
        damage += fighterSkill.damage;

        // ���� �� ������ ����
        if (enemyFighter.fighterAction == FighterAction.Guard)
        {
            damage = fighterSkill.damage * fighterSkill.absorptionRate / 100;

            isGuard = true;

            cantInputTime = 0.5f;

            SetAction(FighterAction.None);
        }
        // �� �÷��̾ ���� �ִϸ��̼��� ��, ī� ������ ���Ͽ� ������ ����
        else if (!(enemyFighter.fighterAction == FighterAction.None || enemyFighter.fighterAction == FighterAction.Hit))
        {
            damage *= status.counterDamageRate;
        }

        // ���� ������ �ñر��� ��, �����ð�+0.5���� �Է� �Ұ� �ð��� ������ �����Ѵ�.
        float lethalMoveCantInputTime = 0;
        if (fighterAction == FighterAction.Ultimate)
        {
            lethalMoveCantInputTime = ultimate.length + 0.5f;
        }

        enemyFighter.currentHP -= damage;
        enemyFighter.Hit(isGuard, transform.rotation.y, lethalMoveCantInputTime);

        currentUltimateGage = Mathf.Clamp(currentUltimateGage + 10, 0, 100);
        enemyFighter.currentUltimateGage = Mathf.Clamp(enemyFighter.currentUltimateGage + 5, 0, 100);
    }
    #endregion
}
