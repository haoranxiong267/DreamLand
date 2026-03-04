using UnityEngine;

public class MapSwitcher : MonoBehaviour
{
    // Assign three map prefabs in the Inspector
    public GameObject[] mapPrefabs;  // Array to store three map prefabs in order

    private GameObject currentMap;   // Currently displayed map
    private int currentMapIndex = 0; // Current map index

    void Start()
    {
        // Load the first map (Grass) when the game starts
        LoadMap(0);  // 0=Grass, 1=Ice, 2=Lava
    }

    // Load map with specified index
    public void LoadMap(int mapIndex)
    {
        // Ensure the index is valid
        if (mapIndex < 0 || mapIndex >= mapPrefabs.Length)
        {
            Debug.LogError("Invalid map index: " + mapIndex);
            return;
        }

        // Destroy the current map
        if (currentMap != null)
        {
            Destroy(currentMap);
        }

        // Instantiate the new map
        currentMap = Instantiate(mapPrefabs[mapIndex], Vector3.zero, Quaternion.identity);
        currentMapIndex = mapIndex;

        Debug.Log("Switched to map: " + GetMapName(mapIndex));
    }

    // Switch to the next map (cycle)
    public void NextMap()
    {
        int nextIndex = (currentMapIndex + 1) % mapPrefabs.Length;
        LoadMap(nextIndex);
    }

    // Get map name (for debugging)
    private string GetMapName(int index)
    {
        switch (index)
        {
            case 0: return "Grass Map";
            case 1: return "Ice Map";
            case 2: return "Lava Map";
            default: return "Unknown Map";
        }
    }

    // Get current map index (used by teleporter)
    public int GetCurrentMapIndex()
    {
        return currentMapIndex;
    }
}