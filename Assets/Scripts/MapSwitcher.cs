using UnityEngine;

public class MapSwitcher : MonoBehaviour
{
    public GameObject[] mapPrefabs;
    private GameObject currentMap;
    private int currentMapIndex = 0;

    void Start()
    {
        LoadMap(0);
    }

    public void LoadMap(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= mapPrefabs.Length)
        {
            Debug.LogError("Invalid map index: " + mapIndex);
            return;
        }

        if (currentMap != null)
        {
            Destroy(currentMap);
        }

        currentMap = Instantiate(mapPrefabs[mapIndex], Vector3.zero, Quaternion.identity);
        currentMapIndex = mapIndex;

        Debug.Log("Switching map to: " + GetMapName(mapIndex));

        // --- Notify GlobalEnemyManager to load enemies for the new map ---
        GlobalEnemyManager enemyManager = FindObjectOfType<GlobalEnemyManager>();
        if (enemyManager != null)
        {
            // Pass the current map instance to GlobalEnemyManager
            enemyManager.OnMapSwitched(mapIndex, currentMap);
        }
        else
        {
            Debug.LogWarning("GlobalEnemyManager not found, cannot switch enemy maps");
        }

        // +++ NEW: Trigger random buff selection after map switch +++
        // Note: If you want to skip selection after the first map, add condition like if (mapIndex > 0)
        BuffManager buffManager = FindObjectOfType<BuffManager>();
        if (buffManager != null)
        {
            // Add a delay to let the scene settle before showing UI
            Invoke(nameof(TriggerBuffSelection), 1.0f);
        }
    }

    public void NextMap()
    {
        int nextIndex = (currentMapIndex + 1) % mapPrefabs.Length;
        LoadMap(nextIndex);
    }

    private string GetMapName(int index)
    {
        switch (index)
        {
            case 0: return "Grassland Map";
            case 1: return "Forest Map";
            case 2: return "Snow Map";
            default: return "Unknown Map";
        }
    }

    public int GetCurrentMapIndex()
    {
        return currentMapIndex;
    }

    // New method to trigger buff selection
    private void TriggerBuffSelection()
    {
        BuffManager buffManager = FindObjectOfType<BuffManager>();
        if (buffManager != null)
        {
            buffManager.ShowRandomBuffSelection();
        }
    }
}