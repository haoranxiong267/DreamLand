using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Essential for TextMeshPro

public class PlayerStats : MonoBehaviour
{
    [Header("Player Attributes")]
    [Tooltip("Maximum health of the player")]
    public int maxHealth = 100;

    [Tooltip("Current health of the player")]
    [SerializeField] private int currentHealth;

    [Tooltip("Base attack power of the player")]
    public int baseAttackPower = 10;

    [Tooltip("Current attack power (including bonuses)")]
    [SerializeField] private int currentAttackPower;

    [Header("UI Component References - Drag & Drop Here")]
    [Tooltip("Attack power text display (TextMeshPro)")]
    public TMP_Text attackText;  // Must use TMP_Text for TextMeshPro

    [Tooltip("Health text display (TextMeshPro)")]
    public TMP_Text healthText;  // Must use TMP_Text for TextMeshPro

    [Tooltip("Health bar slider component")]
    public Slider healthSlider;  // Standard Unity UI Slider

    [Header("Debug Options")]
    [Tooltip("Enable debug logging")]
    public bool enableDebug = true;

    // Public properties for external access
    public int CurrentHealth
    {
        get { return currentHealth; }
        set
        {
            int oldHealth = currentHealth;
            currentHealth = Mathf.Clamp(value, 0, maxHealth);

            if (oldHealth != currentHealth)
            {
                UpdateHealthUI();

                if (enableDebug)
                    Debug.Log($"Player health changed: {oldHealth} -> {currentHealth}");
            }
        }
    }

    public int CurrentAttackPower
    {
        get { return currentAttackPower; }
        set
        {
            int oldAttack = currentAttackPower;
            currentAttackPower = Mathf.Max(0, value);

            if (oldAttack != currentAttackPower)
            {
                UpdateAttackUI();

                if (enableDebug)
                    Debug.Log($"Player attack power changed: {oldAttack} -> {currentAttackPower}");
            }
        }
    }

    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;

    void Start()
    {
        // Initialize player stats
        InitializeStats();

        // Update UI display
        UpdateAllUI();

        if (enableDebug)
            Debug.Log("PlayerStats initialized successfully");
    }

    // Initialize player attributes
    void InitializeStats()
    {
        currentHealth = maxHealth;
        currentAttackPower = baseAttackPower;
    }

    // Update all UI elements
    void UpdateAllUI()
    {
        UpdateHealthUI();
        UpdateAttackUI();
    }

    // Update attack power UI display
    public void UpdateAttackUI()
    {
        if (attackText != null)
        {
            attackText.text = currentAttackPower.ToString();
        }
        else if (enableDebug)
        {
            Debug.LogWarning("Attack Text reference is not assigned! Please drag a TextMeshPro object to the Attack Text field.");
        }
    }

    // Update health UI display
    public void UpdateHealthUI()
    {
        // Update health text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
        else if (enableDebug)
        {
            Debug.LogWarning("Health Text reference is not assigned! Please drag a TextMeshPro object to the Health Text field.");
        }

        // Update health bar slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        else if (enableDebug)
        {
            Debug.LogWarning("Health Slider reference is not assigned! Please drag a Slider object to the Health Slider field.");
        }
    }

    // Player takes damage
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        int actualDamage = Mathf.Min(damage, currentHealth);
        CurrentHealth -= actualDamage;

        if (enableDebug)
            Debug.Log($"Player took {actualDamage} damage, remaining health: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    // Player heals
    public void Heal(int healAmount)
    {
        if (healAmount <= 0) return;

        CurrentHealth += healAmount;

        if (enableDebug)
            Debug.Log($"Player healed {healAmount} health, current health: {CurrentHealth}");
    }

    // Increase attack power (for ability selection system)
    public void IncreaseAttackPower(int amount)
    {
        if (amount <= 0) return;

        CurrentAttackPower += amount;

        if (enableDebug)
            Debug.Log($"Player attack power increased by {amount}, current: {CurrentAttackPower}");
    }

    // Increase maximum health
    public void IncreaseMaxHealth(int amount)
    {
        if (amount <= 0) return;

        maxHealth += amount;
        CurrentHealth += amount; // Also heal by the same amount

        if (enableDebug)
            Debug.Log($"Player max health increased by {amount}, current: {maxHealth}");
    }

    // Reset to initial state (for restarting game)
    public void ResetStats()
    {
        maxHealth = 100;
        baseAttackPower = 10;
        InitializeStats();
        UpdateAllUI();

        if (enableDebug)
            Debug.Log("Player stats have been reset");
    }

    // Player death
    void Die()
    {
        if (enableDebug)
            Debug.Log("Player died!");

        // Add death logic here
    }

    // Debug methods for testing in editor
    [ContextMenu("Test: Take 10 Damage")]
    void TestTakeDamage()
    {
        TakeDamage(10);
    }

    [ContextMenu("Test: Heal 20 Health")]
    void TestHeal()
    {
        Heal(20);
    }

    [ContextMenu("Test: Increase Attack by 5")]
    void TestIncreaseAttack()
    {
        IncreaseAttackPower(5);
    }

    [ContextMenu("Test: Increase Max Health by 30")]
    void TestIncreaseMaxHealth()
    {
        IncreaseMaxHealth(30);
    }

    [ContextMenu("Test: Reset Stats")]
    void TestResetStats()
    {
        ResetStats();
    }

    [ContextMenu("Show Current Status")]
    void LogCurrentStatus()
    {
        Debug.Log($"Player Status - Health: {currentHealth}/{maxHealth}, Attack: {currentAttackPower}, Health %: {HealthPercentage:P0}");
    }
}