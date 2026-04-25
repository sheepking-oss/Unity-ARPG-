using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }
    
    [Header("副本配置")]
    public List<DungeonConfig> dungeons;
    public string mainCitySceneName = "MainCity";
    
    [Header("当前状态")]
    public DungeonConfig currentDungeon;
    public bool inDungeon = false;
    public int playerDeaths = 0;
    public float dungeonTime = 0f;
    public List<MonsterController> dungeonMonsters;
    public MonsterController dungeonBoss;
    
    [Header("UI引用")]
    public DungeonUI dungeonUI;
    
    private Dictionary<int, DungeonConfig> dungeonDictionary;
    private bool dungeonCompleted = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        InitializeDungeonDictionary();
    }
    
    private void Update()
    {
        if (inDungeon)
        {
            dungeonTime += Time.deltaTime;
            
            if (currentDungeon != null && dungeonTime >= currentDungeon.timeLimit)
            {
                DungeonFailed("时间耗尽！");
            }
            
            if (dungeonUI != null)
            {
                dungeonUI.UpdateTimer(dungeonTime, currentDungeon.timeLimit);
            }
        }
    }
    
    private void InitializeDungeonDictionary()
    {
        dungeonDictionary = new Dictionary<int, DungeonConfig>();
        
        foreach (var dungeon in dungeons)
        {
            if (!dungeonDictionary.ContainsKey(dungeon.dungeonId))
            {
                dungeonDictionary.Add(dungeon.dungeonId, dungeon);
            }
            else
            {
                Debug.LogWarning($"重复的副本ID：{dungeon.dungeonId}");
            }
        }
    }
    
    public DungeonConfig GetDungeonById(int id)
    {
        if (dungeonDictionary.TryGetValue(id, out DungeonConfig config))
        {
            return config;
        }
        return null;
    }
    
    public bool CanEnterDungeon(DungeonConfig dungeon)
    {
        if (dungeon == null) return false;
        
        if (CharacterManager.Instance == null || CharacterManager.Instance.playerCharacter == null)
        {
            return false;
        }
        
        if (CharacterManager.Instance.playerCharacter.level < dungeon.recommendedLevel)
        {
            return false;
        }
        
        return true;
    }
    
    public void EnterDungeon(DungeonConfig dungeon)
    {
        if (!CanEnterDungeon(dungeon))
        {
            Debug.LogWarning("无法进入副本！");
            return;
        }
        
        currentDungeon = dungeon;
        inDungeon = true;
        playerDeaths = 0;
        dungeonTime = 0f;
        dungeonCompleted = false;
        dungeonMonsters = new List<MonsterController>();
        dungeonBoss = null;
        
        StartCoroutine(LoadDungeonScene(dungeon));
    }
    
    private IEnumerator LoadDungeonScene(DungeonConfig dungeon)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(dungeon.sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        InitializeDungeon();
    }
    
    private void InitializeDungeon()
    {
        if (currentDungeon == null) return;
        
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.SpawnMonstersFromSpawnList(currentDungeon.normalMonsters);
            MonsterManager.Instance.SpawnMonstersFromSpawnList(currentDungeon.eliteMonsters);
            
            if (currentDungeon.bossMonster != null)
            {
                Transform bossSpawnPoint = FindBossSpawnPoint();
                if (bossSpawnPoint != null)
                {
                    dungeonBoss = MonsterManager.Instance.SpawnMonster(
                        currentDungeon.bossMonster,
                        bossSpawnPoint.position,
                        bossSpawnPoint.rotation
                    );
                }
            }
        }
        
        SpawnPlayerInDungeon();
        
        if (dungeonUI != null)
        {
            dungeonUI.Show();
            dungeonUI.UpdateDungeonInfo(currentDungeon);
        }
        
        Debug.Log($"进入副本：{currentDungeon.dungeonName}");
    }
    
    private Transform FindBossSpawnPoint()
    {
        GameObject bossSpawn = GameObject.Find("BossSpawnPoint");
        if (bossSpawn != null)
        {
            return bossSpawn.transform;
        }
        
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length > 0)
        {
            return spawnPoints[spawnPoints.Length - 1].transform;
        }
        
        return null;
    }
    
    private void SpawnPlayerInDungeon()
    {
        if (CharacterManager.Instance == null) return;
        
        GameObject playerSpawn = GameObject.Find("PlayerSpawnPoint");
        Vector3 spawnPosition = playerSpawn != null ? playerSpawn.transform.position : Vector3.zero;
        
        CharacterManager.Instance.SpawnPlayer(spawnPosition);
    }
    
    public void CheckDungeonCompletion()
    {
        if (!inDungeon || currentDungeon == null || dungeonCompleted) return;
        
        bool allMonstersDead = true;
        
        if (MonsterManager.Instance != null)
        {
            allMonstersDead = MonsterManager.Instance.GetAliveMonsterCount() == 0;
        }
        
        if (dungeonBoss != null && !dungeonBoss.isDead)
        {
            allMonstersDead = false;
        }
        
        if (allMonstersDead)
        {
            DungeonCompleted();
        }
    }
    
    private void DungeonCompleted()
    {
        dungeonCompleted = true;
        
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.AddExperience(currentDungeon.completionExperience);
            CharacterManager.Instance.AddGold(currentDungeon.completionGold);
        }
        
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnDungeonCleared(currentDungeon.dungeonId);
        }
        
        if (dungeonUI != null)
        {
            dungeonUI.ShowCompletion(currentDungeon);
        }
        
        Debug.Log($"副本完成：{currentDungeon.dungeonName}");
        Debug.Log($"获得经验：{currentDungeon.completionExperience}，金币：{currentDungeon.completionGold}");
    }
    
    private void DungeonFailed(string reason)
    {
        if (dungeonUI != null)
        {
            dungeonUI.ShowFailure(reason);
        }
        
        Debug.Log($"副本失败：{reason}");
        
        StartCoroutine(ReturnToMainCityAfterDelay(3f));
    }
    
    public void OnPlayerDeath()
    {
        if (!inDungeon || currentDungeon == null) return;
        
        playerDeaths++;
        
        if (dungeonUI != null)
        {
            dungeonUI.UpdateDeaths(playerDeaths, currentDungeon.maxDeaths);
        }
        
        if (playerDeaths >= currentDungeon.maxDeaths)
        {
            DungeonFailed("死亡次数过多！");
        }
    }
    
    public void ReturnToMainCity()
    {
        inDungeon = false;
        currentDungeon = null;
        dungeonMonsters.Clear();
        dungeonBoss = null;
        
        if (dungeonUI != null)
        {
            dungeonUI.Hide();
        }
        
        SceneManager.LoadScene(mainCitySceneName);
    }
    
    private IEnumerator ReturnToMainCityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToMainCity();
    }
    
    public List<DungeonConfig> GetAvailableDungeons()
    {
        List<DungeonConfig> available = new List<DungeonConfig>();
        
        foreach (var dungeon in dungeons)
        {
            if (CanEnterDungeon(dungeon))
            {
                available.Add(dungeon);
            }
        }
        
        return available;
    }
}
