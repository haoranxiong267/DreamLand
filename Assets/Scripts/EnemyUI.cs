using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUI : MonoBehaviour
{
    [Header("UI组件引用")]
    public Slider healthSlider;
    public TMP_Text attackPowerText;
    public TMP_Text healthText;
    public Image healthFillImage;

    [Header("字体设置")]
    [Tooltip("字体大小")]
    public int fontSize = 20;

    [Header("Canvas设置")]
    [Tooltip("Canvas缩放")]
    public float canvasScale = 0.015f;

    [Header("目标与位置")]
    [Tooltip("要跟随的敌人Transform")]
    public Transform target;

    [Tooltip("在敌人头顶的偏移量")]
    public Vector3 worldOffset = new Vector3(0, 2.5f, 0);

    // 组件引用
    private Camera mainCamera;
    private Canvas canvas;
    private RectTransform rectTransform;
    private EnemyAI enemyAI; // 用于获取敌人的属性

    void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        // 1. 配置Canvas
        if (canvas != null)
        {
            canvas.worldCamera = mainCamera;
            canvas.sortingOrder = 100; // 提高渲染排序
        }

        // 2. 调整Canvas缩放
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(300, 100);
            rectTransform.localScale = new Vector3(canvasScale, canvasScale, canvasScale);
        }

        // 3. 配置TextMeshPro字体
        ConfigureTextMeshPro();

        if (target != null)
        {
            // 获取敌人的EnemyAI组件
            enemyAI = target.GetComponent<EnemyAI>();

            // 初始化UI位置
            transform.position = target.position + worldOffset;

            // 如果找到了敌人AI，初始化UI显示
            if (enemyAI != null)
            {
                UpdateAttackUI(enemyAI.attackPower);
                UpdateHealthUI(enemyAI.CurrentHealth, enemyAI.MaxHealth);
            }
        }
    }

    void ConfigureTextMeshPro()
    {
        // 配置攻击力文本
        if (attackPowerText != null)
        {
            attackPowerText.fontSize = fontSize;
            attackPowerText.enableAutoSizing = false;
            attackPowerText.fontStyle = FontStyles.Bold;
            attackPowerText.color = Color.white;

            // 添加轮廓效果，增强可读性
            attackPowerText.outlineWidth = 0.2f;
            attackPowerText.outlineColor = Color.black;
        }

        // 配置生命值文本
        if (healthText != null)
        {
            healthText.fontSize = fontSize - 2;
            healthText.enableAutoSizing = false;
            healthText.fontStyle = FontStyles.Normal;
            healthText.color = Color.white;
            healthText.outlineWidth = 0.2f;
            healthText.outlineColor = Color.black;
        }
    }

    void Update()
    {
        if (target == null)
        {
            // 如果目标不存在，销毁UI
            Destroy(gameObject);
            return;
        }

        // 保持UI在目标头顶
        transform.position = target.position + worldOffset;

        // 始终面向摄像机
        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }

    public void UpdateAttackUI(int attackPower)
    {
        if (attackPowerText != null)
        {
            attackPowerText.text = attackPower.ToString();
        }
    }

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            // 根据血量百分比改变血条颜色
            float healthPercent = (float)currentHealth / maxHealth;
            if (healthFillImage != null)
            {
                // 使用颜色渐变
                if (healthPercent > 0.6f)
                    healthFillImage.color = Color.green;
                else if (healthPercent > 0.3f)
                    healthFillImage.color = Color.yellow;
                else
                    healthFillImage.color = Color.red;
            }
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
}
