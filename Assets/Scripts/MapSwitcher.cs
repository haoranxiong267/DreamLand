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
            Debug.LogError("无效的地图索引: " + mapIndex);
            return;
        }

        if (currentMap != null)
        {
            Destroy(currentMap);
        }

        currentMap = Instantiate(mapPrefabs[mapIndex], Vector3.zero, Quaternion.identity);
        currentMapIndex = mapIndex;

        Debug.Log("切换到地图: " + GetMapName(mapIndex));

        // --- 通知全局敌人生成管理器切换地图 ---
        GlobalEnemyManager enemyManager = FindObjectOfType<GlobalEnemyManager>();
        if (enemyManager != null)
        {
            // 传递当前地图实例给GlobalEnemyManager
            enemyManager.OnMapSwitched(mapIndex, currentMap);
        }
        else
        {
            Debug.LogWarning("未找到GlobalEnemyManager，敌人不会随地图切换。");
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
            case 0: return "草地地图";
            case 1: return "冰原地图";
            case 2: return "熔岩地图";
            default: return "未知地图";
        }
    }

    public int GetCurrentMapIndex()
    {
        return currentMapIndex;
    }
}