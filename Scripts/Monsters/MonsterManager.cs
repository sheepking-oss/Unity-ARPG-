using UnityEngine;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }
    
    [Header("怪物配置")]
    public List<MonsterConfig> allMonsters;
    
    [Header("生成设置")]
    public float spawnInterval = 10f;
    public int maxMonsters = 10;
    public List<Transform> spawnPoints;
    
    [Header("当前状态")]
    public List<MonsterController> activeMonsters;
    
    private Dictionary<int, MonsterConfig> monsterDictionary;
    private float spawnTimer = 0f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        
        InitializeDictionary();
    }
    
    private void Update()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;
        
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval && activeMonsters.Count < maxMonsters)
        {
            SpawnRandomMonster();
            spawnTimer = 0f;
        }
    }
    
    private void InitializeDictionary()
    {
        monsterDictionary = new Dictionary<int, MonsterConfig>();
        
        foreach (var monster in allMonsters)
        {
            if (!monsterDictionary.ContainsKey(monster.monsterId))
            {
                monsterDictionary.Add(monster.monsterId, monster);
            }
            else
            {
                Debug.LogWarning($"重复的怪物ID：{monster.monsterId}");
            }
        }
    }
    
    public MonsterConfig GetMonsterById(int id)
    {
        if (monsterDictionary.TryGetValue(id, out MonsterConfig config))
        {
            return config;
        }
        return null;
    }
    
    public List<MonsterConfig> GetMonstersByType(MonsterType type)
    {
        List<MonsterConfig> result = new List<MonsterConfig>();
        
        foreach (var monster in allMonsters)
        {
            if (monster.monsterType == type)
            {
                result.Add(monster);
            }
        }
        
        return result;
    }
    
    public List<MonsterConfig> GetMonstersByLevelRange(int minLevel, int maxLevel)
    {
        List<MonsterConfig> result = new List<MonsterConfig>();
        
        foreach (var monster in allMonsters)
        {
            if (monster.level >= minLevel && monster.level <= maxLevel)
            {
                result.Add(monster);
            }
        }
        
        return result;
    }
    
    public MonsterController SpawnMonster(MonsterConfig config, Vector3 position, Quaternion rotation)
    {
        if (config == null || config.monsterPrefab == null)
        {
            Debug.LogError("无效的怪物配置或预制体！");
            return null;
        }
        
        GameObject monsterObject = Instantiate(config.monsterPrefab, position, rotation);
        MonsterController monsterController = monsterObject.GetComponent<MonsterController>();
        
        if (monsterController != null)
        {
            monsterController.config = config;
            monsterController.Initialize();
            
            activeMonsters.Add(monsterController);
            
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.RegisterMonster(monsterController);
            }
        }
        
        return monsterController;
    }
    
    public void SpawnRandomMonster()
    {
        if (allMonsters == null || allMonsters.Count == 0)
        {
            Debug.LogWarning("没有可用的怪物配置！");
            return;
        }
        
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("没有可用的生成点！");
            return;
        }
        
        MonsterConfig randomMonster = allMonsters[Random.Range(0, allMonsters.Count)];
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        
        SpawnMonster(randomMonster, randomSpawnPoint.position, randomSpawnPoint.rotation);
    }
    
    public void SpawnMonstersFromSpawnList(List<MonsterSpawn> spawnList)
    {
        if (spawnList == null) return;
        
        foreach (var spawn in spawnList)
        {
            int count = Random.Range(spawn.minCount, spawn.maxCount + 1);
            
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPosition = Vector3.zero;
                
                if (spawn.spawnPoints != null && spawn.spawnPoints.Count > 0)
                {
                    Transform spawnPoint = spawn.spawnPoints[Random.Range(0, spawn.spawnPoints.Count)];
                    spawnPosition = spawnPoint.position;
                }
                else
                {
                    spawnPosition = transform.position + Random.insideUnitSphere * 5f;
                    spawnPosition.y = 0;
                }
                
                SpawnMonster(spawn.monsterConfig, spawnPosition, Quaternion.identity);
            }
        }
    }
    
    public void KillAllMonsters()
    {
        foreach (var monster in activeMonsters)
        {
            if (monster != null && !monster.isDead)
            {
                monster.Die();
            }
        }
    }
    
    public void RemoveMonster(MonsterController monster)
    {
        activeMonsters.Remove(monster);
        
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.UnregisterMonster(monster);
        }
    }
    
    public int GetAliveMonsterCount()
    {
        int count = 0;
        foreach (var monster in activeMonsters)
        {
            if (monster != null && !monster.isDead)
            {
                count++;
            }
        }
        return count;
    }
    
    public List<MonsterController> GetAliveMonsters()
    {
        List<MonsterController> aliveMonsters = new List<MonsterController>();
        
        foreach (var monster in activeMonsters)
        {
            if (monster != null && !monster.isDead)
            {
                aliveMonsters.Add(monster);
            }
        }
        
        return aliveMonsters;
    }
}
