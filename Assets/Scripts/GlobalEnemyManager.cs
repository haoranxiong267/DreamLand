using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GlobalEnemyManager : MonoBehaviour
{
    public static GlobalEnemyManager Instance;

    [System.Serializable]
    public class MapEnemySettings
    {
        [Tooltip("地图索引 (0:草地, 1:冰原, 2:熔岩)")]
        public int mapIndex;

        [Tooltip("这个地图可用的敌人预制体")]
        public GameObject[] enemyPrefabsForThisMap;

        [Tooltip("这个地图实际使用的生成点数量（从所有找到的生成点中随机选择）")]
        [Range(1, 3)]
        public int spawnPointsToUse = 1;

        [Tooltip("是否在每次生成时随机选择敌人")]
        public bool randomEnemySelection = true;
    }

    [Header("各地图敌人配置")]
    public MapEnemySettings[] allMapSettings;

    [Header("生成点标签设置")]
    [Tooltip("用于标记生成点的标签名称")]
    public string spawnPointTag = "EnemySpawnPoint";

    [Header("UI配置")]
    [Tooltip("敌人UI预制体，将在敌人生成时附加到敌人头顶")]
    public GameObject enemyUIPrefab; // 在Inspector中，将EnemyUI_Prefab拖入这里

    [Header("当前状态")]
    private int currentMapIndex = -1;
    private List<GameObject> currentEnemies = new List<GameObject>();

    void Awake()
    {
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
        // 游戏开始时，由MapSwitcher触发生成敌人
    }

    public void OnMapSwitched(int newMapIndex, GameObject currentMapInstance)
    {
        Debug.Log($"地图切换至 {newMapIndex}, 重新生成敌人...");

        // 1. 清理旧地图的所有敌人
        ClearAllEnemies();

        // 2. 生成新地图的敌人
        SpawnEnemiesForMap(newMapIndex, currentMapInstance);

        currentMapIndex = newMapIndex;
    }

    void SpawnEnemiesForMap(int mapIndex, GameObject currentMapInstance)
    {
        MapEnemySettings settings = GetSettingsForMap(mapIndex);
        if (settings == null || settings.enemyPrefabsForThisMap.Length == 0)
        {
            Debug.LogWarning($"地图 {mapIndex} 没有配置敌人。");
            return;
        }

        // 动态查找当前地图中的所有生成点
        List<Transform> allSpawnPoints = FindAllSpawnPointsInMap(currentMapInstance);

        if (allSpawnPoints.Count == 0)
        {
            Debug.LogError($"地图 {mapIndex} 没有找到生成点！请确保生成点有正确的标签。");
            return;
        }

        // 计算实际要使用的生成点数量
        int pointsToUse = Mathf.Min(settings.spawnPointsToUse, allSpawnPoints.Count);

        // 随机选择指定数量的生成点
        List<Transform> selectedSpawnPoints = SelectRandomSpawnPoints(allSpawnPoints, pointsToUse);

        Debug.Log($"在地图 {mapIndex} 中找到 {allSpawnPoints.Count} 个生成点，将使用 {pointsToUse} 个");

        // 在每个选中的生成点生成敌人
        for (int i = 0; i < selectedSpawnPoints.Count; i++)
        {
            Transform spawnPoint = selectedSpawnPoints[i];

            // 选择敌人预制体
            GameObject enemyPrefab = SelectEnemyPrefab(settings, i);

            if (enemyPrefab == null)
            {
                Debug.LogError($"没有可用的敌人预制体！");
                continue;
            }

            Vector3 spawnPos = spawnPoint.position;
            spawnPos.y = 0; // 确保在地面上

            // 创建敌人
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            newEnemy.transform.parent = this.transform;
            currentEnemies.Add(newEnemy);

            Debug.Log($"在生成点 {spawnPoint.name} 生成了敌人: {enemyPrefab.name}");

            // 获取EnemyAI组件
            EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();

            if (enemyAI != null)
            {
                // === 修改：为敌人创建并绑定UI ===
                if (enemyUIPrefab != null)
                {
                    // 实例化UI，位置在敌人头顶
                    GameObject uiInstance = Instantiate(enemyUIPrefab, spawnPos + new Vector3(0, 2.5f, 0), Quaternion.identity);
                    EnemyUI enemyUI = uiInstance.GetComponent<EnemyUI>();
                    if (enemyUI != null)
                    {
                        // 关键修改：将UI设为敌人的子对象，而不是GameManager的子对象
                        // 这样当敌人被销毁时，UI也会自动被销毁
                        uiInstance.transform.SetParent(newEnemy.transform);

                        // 绑定敌人为目标
                        enemyUI.target = newEnemy.transform;

                        // 将UI引用传递给EnemyAI
                        enemyAI.enemyUI = enemyUI;

                        // 初始化UI数据
                        enemyUI.UpdateAttackUI(enemyAI.attackPower);
                        enemyUI.UpdateHealthUI(enemyAI.CurrentHealth, enemyAI.MaxHealth);
                    }
                }

                // 为敌人设置玩家目标
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    enemyAI.playerTarget = player.transform;
                    Debug.Log($"为 {enemyPrefab.name} 设置玩家目标: {player.name}");
                }
                else
                {
                    Debug.LogWarning("未找到玩家对象！");
                }
            }
            else
            {
                Debug.LogWarning($"生成的敌人 {enemyPrefab.name} 没有EnemyAI组件！");
            }
        }
    }

    // 在指定地图实例中查找所有生成点
    List<Transform> FindAllSpawnPointsInMap(GameObject mapInstance)
    {
        List<Transform> spawnPoints = new List<Transform>();

        // 查找所有子对象，包括嵌套的子对象
        Transform[] allChildren = mapInstance.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            // 如果对象标签匹配
            if (child.CompareTag(spawnPointTag))
            {
                spawnPoints.Add(child);
                Debug.Log($"找到生成点: {child.name} (父对象: {child.parent.name})");
            }
        }

        return spawnPoints;
    }

    // 随机选择指定数量的生成点
    List<Transform> SelectRandomSpawnPoints(List<Transform> allPoints, int count)
    {
        if (count >= allPoints.Count)
        {
            return new List<Transform>(allPoints);
        }

        // 创建副本避免修改原列表
        List<Transform> availablePoints = new List<Transform>(allPoints);
        List<Transform> selectedPoints = new List<Transform>();

        for (int i = 0; i < count; i++)
        {
            if (availablePoints.Count == 0) break;

            int randomIndex = Random.Range(0, availablePoints.Count);
            selectedPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);
        }

        return selectedPoints;
    }

    // 选择敌人预制体
    GameObject SelectEnemyPrefab(MapEnemySettings settings, int spawnPointIndex)
    {
        if (settings.enemyPrefabsForThisMap.Length == 0)
            return null;

        if (settings.randomEnemySelection)
        {
            // 随机选择
            int randomIndex = Random.Range(0, settings.enemyPrefabsForThisMap.Length);
            return settings.enemyPrefabsForThisMap[randomIndex];
        }
        else
        {
            // 按顺序选择（循环使用）
            int index = spawnPointIndex % settings.enemyPrefabsForThisMap.Length;
            return settings.enemyPrefabsForThisMap[index];
        }
    }

    void ClearAllEnemies()
    {
        foreach (GameObject enemy in currentEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        currentEnemies.Clear();
    }

    MapEnemySettings GetSettingsForMap(int index)
    {
        foreach (var setting in allMapSettings)
        {
            if (setting.mapIndex == index) return setting;
        }
        return null;
    }
}