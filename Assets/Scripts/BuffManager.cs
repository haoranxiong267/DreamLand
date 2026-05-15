using UnityEngine;
using System.Collections.Generic;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance;

    [Header("Buff Library")]
    public List<PlayerBuff> allBuffs; // Drag all your PlayerBuff assets here in Inspector

    [Header("UI Prefab")]
    public GameObject buffSelectionUIPrefab; // Drag the BuffSelectionCanvas prefab here

    private BuffSelectionUI currentUIInstance; // Current UI instance
    private PlayerStats playerStats; // Reference to player stats
    private PlayerCombat playerCombat; // Reference to player combat
    private PlayerController playerController; // Reference to player controller

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Trigger buff selection at game start
        // Delay one frame to ensure other managers (e.g., PlayerStats) are initialized
        Invoke(nameof(ShowBuffSelectionAtGameStart), 0.5f);
    }

    void ShowBuffSelectionAtGameStart()
    {
        ShowRandomBuffSelection();
    }

    // Public method: trigger random buff selection (can be called by MapSwitcher)
    public void ShowRandomBuffSelection()
    {
        if (allBuffs == null || allBuffs.Count < 3)
        {
            Debug.LogError("BuffManager: Not enough buffs (need at least 3)!");
            return;
        }

        // 1. Randomly select 3 unique buffs
        List<PlayerBuff> selectedBuffs = new List<PlayerBuff>();
        List<PlayerBuff> availableBuffs = new List<PlayerBuff>(allBuffs);

        for (int i = 0; i < 3; i++)
        {
            if (availableBuffs.Count == 0) break;
            int randomIndex = Random.Range(0, availableBuffs.Count);
            selectedBuffs.Add(availableBuffs[randomIndex]);
            availableBuffs.RemoveAt(randomIndex);
        }

        // 2. Create UI
        if (buffSelectionUIPrefab != null && currentUIInstance == null)
        {
            GameObject uiObj = Instantiate(buffSelectionUIPrefab);
            currentUIInstance = uiObj.GetComponent<BuffSelectionUI>();
            if (currentUIInstance == null)
            {
                Debug.LogError("BuffManager: BuffSelectionUI component not found on UI prefab!");
                return;
            }
        }

        // 3. Get player component references
        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();
        if (playerCombat == null)
            playerCombat = FindObjectOfType<PlayerCombat>();
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        // 4. Set up UI and listen for selection
        currentUIInstance.onBuffSelected = (selectedBuff) => ApplyBuffToPlayer(selectedBuff);
        currentUIInstance.ShowBuffSelection(selectedBuffs.ToArray());
    }

    // Apply buff effect to player
    private void ApplyBuffToPlayer(PlayerBuff buff)
    {
        if (buff == null || playerStats == null) return;

        Debug.Log($"Applying buff [{buff.buffName}] to player");

        switch (buff.buffType)
        {
            case PlayerBuff.BuffType.IncreaseMaxHP:
                playerStats.IncreaseMaxHealth((int)buff.value);
                break;
            case PlayerBuff.BuffType.IncreaseAttack:
                playerStats.IncreaseAttackPower((int)buff.value);
                if (playerCombat != null)
                    playerCombat.IncreaseAttackPower((int)buff.value);
                break;
            case PlayerBuff.BuffType.IncreaseAttackRange:
                if (playerCombat != null)
                    playerCombat.IncreaseAttackRange(buff.value);
                break;
            case PlayerBuff.BuffType.ReduceAttackCooldown:
                if (playerCombat != null)
                    playerCombat.ReduceAttackCooldown(buff.value);
                break;
            case PlayerBuff.BuffType.IncreaseMoveSpeed:
                if (playerController != null)
                    playerController.moveSpeed += buff.value; // Note: PlayerController needs public moveSpeed or a setter
                break;
            default:
                Debug.LogWarning($"Unknown buff type: {buff.buffType}");
                break;
        }

        // Destroy UI instance after applying
        if (currentUIInstance != null)
        {
            Destroy(currentUIInstance.gameObject);
            currentUIInstance = null;
        }
    }
}