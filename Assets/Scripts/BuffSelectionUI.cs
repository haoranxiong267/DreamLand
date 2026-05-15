using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class BuffSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject selectionPanel; // The entire selection panel
    public Button[] buffButtons; // Array of 3 buttons, drag in order (0,1,2) in Inspector
    public Image[] buffIconImages; // Corresponding icon Images
    public TMP_Text[] buffNameTexts; // Corresponding name Texts
    public TMP_Text[] buffDescriptionTexts; // Corresponding description Texts

    [Header("Canvas Group")]
    public CanvasGroup mainCanvasGroup; // Canvas Group component for fade effects

    [Header("Events")]
    public Action<PlayerBuff> onBuffSelected; // Event triggered when a buff is selected

    private PlayerBuff[] currentBuffs; // The 3 buffs currently displayed

    void Awake()
    {
        // Ensure Canvas Group exists
        if (mainCanvasGroup == null)
        {
            mainCanvasGroup = GetComponent<CanvasGroup>();
            if (mainCanvasGroup == null)
            {
                mainCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        // Hide panel at start
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // Add listeners to each button
        for (int i = 0; i < buffButtons.Length; i++)
        {
            int index = i; // Important: closure capture
            buffButtons[i].onClick.AddListener(() => OnBuffButtonClicked(index));
        }
    }

    // Show selection UI with 3 random buffs
    public void ShowBuffSelection(PlayerBuff[] buffsToShow)
    {
        if (buffsToShow == null || buffsToShow.Length != buffButtons.Length)
        {
            Debug.LogError("BuffSelectionUI: Invalid buff array provided!");
            return;
        }

        currentBuffs = buffsToShow;

        // Make sure UI is fully visible
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 1f;
            mainCanvasGroup.interactable = true;
            mainCanvasGroup.blocksRaycasts = true;
        }

        selectionPanel.SetActive(true);

        // Update each button's UI
        for (int i = 0; i < buffButtons.Length; i++)
        {
            if (buffsToShow[i] != null)
            {
                buffIconImages[i].sprite = buffsToShow[i].buffIcon;
                buffNameTexts[i].text = buffsToShow[i].buffName;
                buffDescriptionTexts[i].text = buffsToShow[i].buffDescription;
            }
            else
            {
                // If no buff, disable button
                buffButtons[i].interactable = false;
                buffNameTexts[i].text = "None";
                buffDescriptionTexts[i].text = "";
            }
        }

        // Pause game
        Time.timeScale = 0f;
    }

    // Button click event
    private void OnBuffButtonClicked(int buttonIndex)
    {
        if (currentBuffs == null || buttonIndex < 0 || buttonIndex >= currentBuffs.Length)
            return;

        PlayerBuff selectedBuff = currentBuffs[buttonIndex];
        Debug.Log($"Player selected buff: {selectedBuff.buffName}");

        // Trigger selection event
        onBuffSelected?.Invoke(selectedBuff);

        // Start fade out coroutine
        StartCoroutine(FadeOutAndHide());
    }

    // Fade out and hide UI
    private IEnumerator FadeOutAndHide()
    {
        if (mainCanvasGroup != null)
        {
            float fadeDuration = 0.3f;
            float elapsedTime = 0f;
            float startAlpha = mainCanvasGroup.alpha;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime; // Use unscaled time because game is paused
                mainCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
        }

        // Complete hide
        Hide();
    }

    // Hide UI (e.g., timeout auto-select)
    public void Hide()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // Ensure UI is completely hidden
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 0f;
            mainCanvasGroup.interactable = false;
            mainCanvasGroup.blocksRaycasts = false;
        }

        // Resume game
        Time.timeScale = 1f;
    }
}