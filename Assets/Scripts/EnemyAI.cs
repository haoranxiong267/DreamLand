using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("属性配置")]
    [Tooltip("敌人属性配置文件")]
    public EnemyPropertyConfig propertyConfig;  // 属性配置引用

    [Header("生命值设置")]
    [Tooltip("最大生命值")]
    public int maxHealth = 20;

    [Tooltip("当前生命值")]
    [SerializeField] private int currentHealth;

    [Tooltip("攻击力")]
    public int attackPower = 3;

    [Tooltip("攻击范围")]
    public float attackRange = 2f;

    [Header("移动设置")]
    [Tooltip("移动速度")]
    public float moveSpeed = 3f;

    [Tooltip("巡逻半径")]
    public float patrolRadius = 5f;

    [Header("追逐设置")]
    [Tooltip("开始追逐的距离 - 玩家进入此范围时开始追逐")]
    public float chaseStartRange = 10f;

    [Tooltip("停止追逐的距离 - 玩家超出此范围时停止追逐")]
    public float chaseStopRange = 15f;

    [Header("UI设置")]
    [Tooltip("敌人UI组件引用")]
    public EnemyUI enemyUI;

    [Header("目标设置")]
    [Tooltip("玩家目标的Transform")]
    public Transform playerTarget;

    [Tooltip("起始位置，用于巡逻")]
    private Vector3 startPosition;

    [Header("状态")]
    [Tooltip("当前状态")]
    public EnemyState currentState = EnemyState.Patrol;

    [Tooltip("上次攻击时间")]
    private float lastAttackTime = 0f;

    [Tooltip("攻击冷却时间")]
    public float attackCooldown = 2f;

    // 状态枚举
    public enum EnemyState
    {
        Idle,       // 空闲
        Patrol,     // 巡逻
        Chase,      // 追逐
        Attack,     // 攻击
        Hurt,       // 受伤
        Dead        // 死亡
    }

    // 组件引用
    private Animator animator;

    // 公共属性，方便UI访问
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Start()
    {
        // 初始化组件
        animator = GetComponent<Animator>();

        // 起始位置
        startPosition = transform.position;

        // 从属性配置加载数据
        LoadPropertiesFromConfig();

        // 初始化生命值
        currentHealth = maxHealth;

        // 自动查找玩家目标
        FindPlayerTarget();

        // 如果没找到玩家，尝试其他方式
        if (playerTarget == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                playerTarget = playerGO.transform;
                Debug.Log($"通过Player标签找到: {playerTarget.name}");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: 未找到玩家目标，请确保玩家有Player标签");
            }
        }

        // 自动查找EnemyUI组件（如果未手动设置）
        if (enemyUI == null)
        {
            enemyUI = GetComponentInChildren<EnemyUI>();
            if (enemyUI == null)
            {
                Debug.LogWarning($"{gameObject.name}: 未找到EnemyUI组件，血条UI将不会更新");
            }
        }
    }

    // 从属性配置加载数据
    void LoadPropertiesFromConfig()
    {
        if (propertyConfig != null)
        {
            // 加载基本属性
            maxHealth = propertyConfig.maxHealth;
            attackPower = propertyConfig.attackPower;

            // 加载移动设置
            moveSpeed = propertyConfig.moveSpeed;
            patrolRadius = propertyConfig.patrolRadius;

            // 加载攻击设置
            attackRange = propertyConfig.attackRange;
            attackCooldown = propertyConfig.attackCooldown;

            // 加载追逐设置
            chaseStartRange = propertyConfig.chaseStartRange;
            chaseStopRange = propertyConfig.chaseStopRange;

            Debug.Log($"从配置加载属性: {propertyConfig.name} - 生命:{maxHealth}, 攻击:{attackPower}");
        }
        else
        {
            Debug.Log("未设置属性配置，使用默认值");
        }
    }

    // 自动查找玩家目标
    void FindPlayerTarget()
    {
        if (playerTarget != null)
        {
            return; // 已指定则使用指定的目标
        }

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerTarget = playerGO.transform;
        }
    }

    void Update()
    {
        // 如果死亡则不执行任何逻辑
        if (currentState == EnemyState.Dead)
            return;

        // 状态机逻辑
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior();
                break;
            case EnemyState.Chase:
                ChaseBehavior();
                break;
            case EnemyState.Attack:
                AttackBehavior();
                break;
            case EnemyState.Hurt:
                // 受伤状态持续时间，可以添加计时器
                break;
            default:
                break;
        }

        // 检查是否需要切换追逐状态
        if (currentState != EnemyState.Dead &&
            currentState != EnemyState.Hurt &&
            playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            // 使用可配置的追逐范围
            if (distanceToPlayer <= chaseStartRange && distanceToPlayer > attackRange)
            {
                ChangeState(EnemyState.Chase);
            }
            else if (distanceToPlayer <= attackRange)
            {
                ChangeState(EnemyState.Attack);
            }
            else if (currentState == EnemyState.Chase && distanceToPlayer > chaseStopRange)
            {
                ChangeState(EnemyState.Patrol);
            }
        }
    }

    // 巡逻行为
    void PatrolBehavior()
    {
        // 简单的原地旋转巡逻
        transform.Rotate(0, 20 * Time.deltaTime, 0);

        // 动画
        if (animator != null)
        {
            animator.SetFloat("Speed", 0.5f);
        }
    }

    // 追逐行为
    void ChaseBehavior()
    {
        if (playerTarget == null)
            return;

        // 朝向玩家
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        // 移动
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // 动画
        if (animator != null)
        {
            animator.SetFloat("Speed", 1f);
        }
    }

    // 攻击行为
    void AttackBehavior()
    {
        if (playerTarget == null)
            return;

        // 停止移动
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        // 朝向玩家
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        // 攻击冷却
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            StartAttack();
        }
    }

    // 开始攻击
    void StartAttack()
    {
        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 实际伤害逻辑在动画事件中调用
    }

    // 改变状态
    void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        // 退出当前状态
        ExitState(currentState);

        // 设置新状态
        currentState = newState;
        EnterState(newState);

        Debug.Log($"{gameObject.name} 状态改变: {currentState}");
    }

    // 进入状态
    void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                if (animator != null) animator.SetFloat("Speed", 0f);
                break;
            case EnemyState.Patrol:
                if (animator != null) animator.SetFloat("Speed", 0.5f);
                break;
            case EnemyState.Chase:
                if (animator != null) animator.SetFloat("Speed", 1f);
                break;
            case EnemyState.Attack:
                if (animator != null) animator.SetFloat("Speed", 0f);
                break;
            case EnemyState.Hurt:
                if (animator != null) animator.SetTrigger("Hurt");
                break;
            case EnemyState.Dead:
                if (animator != null) animator.SetTrigger("Die");

                // 禁用碰撞体
                Collider col = GetComponent<Collider>();
                if (col != null) col.enabled = false;

                // 禁用EnemyAI脚本
                this.enabled = false;

                // 一段时间后销毁
                Destroy(gameObject, 3f);
                break;
        }
    }

    // 退出状态
    void ExitState(EnemyState state)
    {
        // 退出状态时的逻辑
    }

    // 受到伤害
    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Dead)
            return;

        // 减少生命值
        currentHealth -= damage;

        Debug.Log($"{gameObject.name} 受到 {damage} 点伤害，剩余: {currentHealth}");

        // 更新UI
        if (enemyUI != null)
        {
            enemyUI.UpdateHealthUI(currentHealth, maxHealth);
        }

        // 生命值小于等于0
        if (currentHealth <= 0)
        {
            ChangeState(EnemyState.Dead);
        }
        else
        {
            // 进入受伤状态
            ChangeState(EnemyState.Hurt);

            // 受伤状态持续时间
            Invoke("RecoverFromHurt", 0.5f);
        }
    }

    // 从受伤状态恢复
    void RecoverFromHurt()
    {
        if (currentState == EnemyState.Hurt)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    // 在动画事件中实际执行攻击
    public void ExecuteAttack()
    {
        Debug.Log($"{gameObject.name} 执行攻击!");

        // 这里可以添加对玩家造成伤害的逻辑
        // 例如：检测前方是否有玩家，有则造成伤害
    }

    // 死亡处理
    public void Die()
    {
        if (currentState != EnemyState.Dead)
        {
            ChangeState(EnemyState.Dead);
        }
    }

    // 在编辑器显示范围
    void OnDrawGizmosSelected()
    {
        // 红色线框显示攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 黄色线框显示开始追逐范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseStartRange);

        // 橙色线框显示停止追逐范围
        Gizmos.color = new Color(1f, 0.5f, 0f); // 橙色
        Gizmos.DrawWireSphere(transform.position, chaseStopRange);

        // 绿色线框显示巡逻范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPosition, patrolRadius);
    }
}