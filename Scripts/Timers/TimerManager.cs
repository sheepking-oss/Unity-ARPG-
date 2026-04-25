using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }
    
    [Header("调试设置")]
    public bool showDebugLogs = true;
    public bool pauseTimersWhenGamePaused = true;
    
    [Header("事件")]
    public event System.Action OnCooldownsUpdated;
    public event System.Action OnBuffsUpdated;
    public event System.Action OnSceneAboutToChange;
    public event System.Action OnSceneChanged;
    
    private Dictionary<int, List<CooldownData>> _playerCooldowns = new Dictionary<int, List<CooldownData>>();
    private Dictionary<int, List<BuffData>> _playerBuffs = new Dictionary<int, List<BuffData>>();
    
    private TimerSnapshot _pendingSnapshot = null;
    private bool _isSceneLoading = false;
    private float _lastUpdateTime;
    private int _currentPlayerId = 0;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        _lastUpdateTime = Time.unscaledTime;
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        
        Log("TimerManager 初始化完成，DontDestroyOnLoad 已设置");
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    private void Update()
    {
        if (_isSceneLoading) return;
        
        float deltaTime = Time.unscaledDeltaTime;
        
        UpdateCooldowns(deltaTime);
        UpdateBuffs(deltaTime);
        
        _lastUpdateTime = Time.unscaledTime;
    }
    
    #region 冷却系统
    
    public void StartCooldown(int skillId, string skillName, float cooldownDuration, int playerId = 0)
    {
        if (cooldownDuration <= 0) return;
        
        if (!_playerCooldowns.ContainsKey(playerId))
        {
            _playerCooldowns[playerId] = new List<CooldownData>();
        }
        
        CooldownData existingCooldown = GetCooldown(skillId, playerId);
        if (existingCooldown != null)
        {
            existingCooldown.remainingCooldown = cooldownDuration;
            existingCooldown.startTime = Time.unscaledTime;
            existingCooldown.isActive = true;
            Log($"[冷却] 刷新冷却: {skillName} (ID:{skillId}), 持续时间: {cooldownDuration}s");
        }
        else
        {
            CooldownData newCooldown = new CooldownData(skillId, skillName, cooldownDuration, playerId);
            _playerCooldowns[playerId].Add(newCooldown);
            Log($"[冷却] 开始冷却: {skillName} (ID:{skillId}), 持续时间: {cooldownDuration}s");
        }
        
        OnCooldownsUpdated?.Invoke();
    }
    
    public float GetCooldownRemaining(int skillId, int playerId = 0)
    {
        CooldownData cooldown = GetCooldown(skillId, playerId);
        return cooldown != null ? Mathf.Max(0f, cooldown.remainingCooldown) : 0f;
    }
    
    public float GetCooldownProgress(int skillId, int playerId = 0)
    {
        CooldownData cooldown = GetCooldown(skillId, playerId);
        return cooldown != null ? cooldown.GetProgress() : 1f;
    }
    
    public bool IsCooldownReady(int skillId, int playerId = 0)
    {
        CooldownData cooldown = GetCooldown(skillId, playerId);
        return cooldown == null || cooldown.IsReady();
    }
    
    public void ResetCooldown(int skillId, int playerId = 0)
    {
        CooldownData cooldown = GetCooldown(skillId, playerId);
        if (cooldown != null)
        {
            cooldown.remainingCooldown = 0f;
            cooldown.isActive = false;
            Log($"[冷却] 重置冷却: {cooldown.skillName} (ID:{skillId})");
            OnCooldownsUpdated?.Invoke();
        }
    }
    
    public void ResetAllCooldowns(int playerId = 0)
    {
        if (_playerCooldowns.ContainsKey(playerId))
        {
            foreach (var cooldown in _playerCooldowns[playerId])
            {
                cooldown.remainingCooldown = 0f;
                cooldown.isActive = false;
            }
            Log($"[冷却] 重置所有冷却 (玩家ID:{playerId})");
            OnCooldownsUpdated?.Invoke();
        }
    }
    
    public List<CooldownData> GetAllCooldowns(int playerId = 0)
    {
        if (_playerCooldowns.ContainsKey(playerId))
        {
            return new List<CooldownData>(_playerCooldowns[playerId]);
        }
        return new List<CooldownData>();
    }
    
    private CooldownData GetCooldown(int skillId, int playerId)
    {
        if (_playerCooldowns.ContainsKey(playerId))
        {
            foreach (var cooldown in _playerCooldowns[playerId])
            {
                if (cooldown.skillId == skillId)
                {
                    return cooldown;
                }
            }
        }
        return null;
    }
    
    private void UpdateCooldowns(float deltaTime)
    {
        bool hasChanges = false;
        
        foreach (var playerKvp in _playerCooldowns)
        {
            var cooldowns = playerKvp.Value;
            for (int i = cooldowns.Count - 1; i >= 0; i--)
            {
                var cooldown = cooldowns[i];
                if (cooldown.isActive && cooldown.remainingCooldown > 0)
                {
                    cooldown.remainingCooldown -= deltaTime;
                    hasChanges = true;
                    
                    if (cooldown.remainingCooldown <= 0)
                    {
                        cooldown.remainingCooldown = 0f;
                        cooldown.isActive = false;
                        Log($"[冷却] 冷却完成: {cooldown.skillName} (ID:{cooldown.skillId})");
                    }
                }
            }
        }
        
        if (hasChanges)
        {
            OnCooldownsUpdated?.Invoke();
        }
    }
    
    #endregion
    
    #region Buff系统
    
    public int ApplyBuff(int buffId, string buffName, ModifierType buffType, float duration, float value, int maxStacks = 1, int playerId = 0)
    {
        if (!_playerBuffs.ContainsKey(playerId))
        {
            _playerBuffs[playerId] = new List<BuffData>();
        }
        
        BuffData existingBuff = GetBuff(buffId, playerId);
        if (existingBuff != null)
        {
            if (existingBuff.maxStacks > 1)
            {
                existingBuff.AddStack();
                Log($"[Buff] 叠加Buff: {buffName} (ID:{buffId}), 当前层数: {existingBuff.stackCount}");
            }
            else
            {
                existingBuff.Refresh();
                Log($"[Buff] 刷新Buff: {buffName} (ID:{buffId}), 持续时间: {duration}s");
            }
            OnBuffsUpdated?.Invoke();
            return existingBuff.stackCount;
        }
        
        BuffData newBuff = new BuffData(buffId, buffName, buffType, duration, value, maxStacks, playerId);
        _playerBuffs[playerId].Add(newBuff);
        
        Log($"[Buff] 应用Buff: {buffName} (ID:{buffId}), 类型:{buffType}, 持续时间:{duration}s, 值:{value}");
        OnBuffsUpdated?.Invoke();
        
        return 1;
    }
    
    public void RemoveBuff(int buffId, int playerId = 0)
    {
        if (_playerBuffs.ContainsKey(playerId))
        {
            for (int i = _playerBuffs[playerId].Count - 1; i >= 0; i--)
            {
                var buff = _playerBuffs[playerId][i];
                if (buff.buffId == buffId)
                {
                    Log($"[Buff] 移除Buff: {buff.buffName} (ID:{buffId})");
                    _playerBuffs[playerId].RemoveAt(i);
                    OnBuffsUpdated?.Invoke();
                    return;
                }
            }
        }
    }
    
    public void RemoveBuffByType(ModifierType buffType, int playerId = 0)
    {
        if (_playerBuffs.ContainsKey(playerId))
        {
            for (int i = _playerBuffs[playerId].Count - 1; i >= 0; i--)
            {
                var buff = _playerBuffs[playerId][i];
                if (buff.buffType == buffType)
                {
                    Log($"[Buff] 按类型移除Buff: {buff.buffName} (类型:{buffType})");
                    _playerBuffs[playerId].RemoveAt(i);
                }
            }
            OnBuffsUpdated?.Invoke();
        }
    }
    
    public void RemoveAllBuffs(int playerId = 0)
    {
        if (_playerBuffs.ContainsKey(playerId))
        {
            Log($"[Buff] 移除所有Buff (玩家ID:{playerId})");
            _playerBuffs[playerId].Clear();
            OnBuffsUpdated?.Invoke();
        }
    }
    
    public BuffData GetBuff(int buffId, int playerId = 0)
    {
        if (_playerBuffs.ContainsKey(playerId))
        {
            foreach (var buff in _playerBuffs[playerId])
            {
                if (buff.buffId == buffId && buff.isActive)
                {
                    return buff;
                }
            }
        }
        return null;
    }
    
    public List<BuffData> GetAllBuffs(int playerId = 0)
    {
        if (_playerBuffs.ContainsKey(playerId))
        {
            return new List<BuffData>(_playerBuffs[playerId]);
        }
        return new List<BuffData>();
    }
    
    public float GetBuffValue(ModifierType buffType, int playerId = 0)
    {
        float totalValue = 0f;
        
        if (_playerBuffs.ContainsKey(playerId))
        {
            foreach (var buff in _playerBuffs[playerId])
            {
                if (buff.buffType == buffType && buff.isActive)
                {
                    totalValue += buff.value * buff.stackCount;
                }
            }
        }
        
        return totalValue;
    }
    
    public bool HasBuff(ModifierType buffType, int playerId = 0)
    {
        if (_playerBuffs.ContainsKey(playerId))
        {
            foreach (var buff in _playerBuffs[playerId])
            {
                if (buff.buffType == buffType && buff.isActive)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    private void UpdateBuffs(float deltaTime)
    {
        bool hasChanges = false;
        
        foreach (var playerKvp in _playerBuffs)
        {
            var buffs = playerKvp.Value;
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                var buff = buffs[i];
                if (buff.isActive && buff.remainingDuration > 0)
                {
                    buff.remainingDuration -= deltaTime;
                    hasChanges = true;
                    
                    if (buff.remainingDuration <= 0)
                    {
                        Log($"[Buff] Buff过期: {buff.buffName} (ID:{buff.buffId})");
                        buffs.RemoveAt(i);
                    }
                }
            }
        }
        
        if (hasChanges)
        {
            OnBuffsUpdated?.Invoke();
        }
    }
    
    #endregion
    
    #region 场景切换管理
    
    private void OnSceneUnloaded(Scene scene)
    {
        Log($"[场景] 场景卸载: {scene.name}，准备保存计时器状态...");
        _isSceneLoading = true;
        OnSceneAboutToChange?.Invoke();
        SaveTimerSnapshot();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Log($"[场景] 场景加载完成: {scene.name}，准备恢复计时器状态...");
        RestoreTimerSnapshot();
        _isSceneLoading = false;
        OnSceneChanged?.Invoke();
    }
    
    private void SaveTimerSnapshot()
    {
        _pendingSnapshot = new TimerSnapshot();
        
        foreach (var playerKvp in _playerCooldowns)
        {
            foreach (var cooldown in playerKvp.Value)
            {
                if (cooldown.isActive && cooldown.remainingCooldown > 0)
                {
                    _pendingSnapshot.cooldownSnapshots.Add(new CooldownSnapshot
                    {
                        skillId = cooldown.skillId,
                        skillName = cooldown.skillName,
                        totalCooldown = cooldown.totalCooldown,
                        remainingCooldown = cooldown.remainingCooldown,
                        ownerId = cooldown.ownerId
                    });
                }
            }
        }
        
        foreach (var playerKvp in _playerBuffs)
        {
            foreach (var buff in playerKvp.Value)
            {
                if (buff.isActive && buff.remainingDuration > 0)
                {
                    _pendingSnapshot.buffSnapshots.Add(new BuffSnapshot
                    {
                        buffId = buff.buffId,
                        buffName = buff.buffName,
                        buffType = buff.buffType,
                        totalDuration = buff.totalDuration,
                        remainingDuration = buff.remainingDuration,
                        value = buff.value,
                        stackCount = buff.stackCount,
                        maxStacks = buff.maxStacks,
                        ownerId = buff.ownerId
                    });
                }
            }
        }
        
        Log($"[快照] 保存计时器快照 - 冷却:{_pendingSnapshot.cooldownSnapshots.Count}, Buff:{_pendingSnapshot.buffSnapshots.Count}");
    }
    
    private void RestoreTimerSnapshot()
    {
        if (_pendingSnapshot == null)
        {
            Log("[快照] 没有待恢复的快照");
            return;
        }
        
        foreach (var cooldownSnap in _pendingSnapshot.cooldownSnapshots)
        {
            if (!_playerCooldowns.ContainsKey(cooldownSnap.ownerId))
            {
                _playerCooldowns[cooldownSnap.ownerId] = new List<CooldownData>();
            }
            
            CooldownData cooldown = new CooldownData(
                cooldownSnap.skillId,
                cooldownSnap.skillName,
                cooldownSnap.totalCooldown,
                cooldownSnap.ownerId
            );
            cooldown.remainingCooldown = cooldownSnap.remainingCooldown;
            cooldown.isActive = cooldown.remainingCooldown > 0;
            
            _playerCooldowns[cooldownSnap.ownerId].Add(cooldown);
        }
        
        foreach (var buffSnap in _pendingSnapshot.buffSnapshots)
        {
            if (!_playerBuffs.ContainsKey(buffSnap.ownerId))
            {
                _playerBuffs[buffSnap.ownerId] = new List<BuffData>();
            }
            
            BuffData buff = new BuffData(
                buffSnap.buffId,
                buffSnap.buffName,
                buffSnap.buffType,
                buffSnap.totalDuration,
                buffSnap.value,
                buffSnap.maxStacks,
                buffSnap.ownerId
            );
            buff.remainingDuration = buffSnap.remainingDuration;
            buff.stackCount = buffSnap.stackCount;
            buff.isActive = buff.remainingDuration > 0;
            
            _playerBuffs[buffSnap.ownerId].Add(buff);
        }
        
        Log($"[快照] 恢复计时器快照 - 冷却:{_pendingSnapshot.cooldownSnapshots.Count}, Buff:{_pendingSnapshot.buffSnapshots.Count}");
        
        _pendingSnapshot = null;
        
        OnCooldownsUpdated?.Invoke();
        OnBuffsUpdated?.Invoke();
    }
    
    #endregion
    
    #region 调试工具
    
    public string GetDebugInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== TimerManager 调试信息 ===");
        sb.AppendLine($"场景加载中: {_isSceneLoading}");
        sb.AppendLine($"当前玩家ID: {_currentPlayerId}");
        
        sb.AppendLine($"\n冷却列表:");
        foreach (var playerKvp in _playerCooldowns)
        {
            sb.AppendLine($"  玩家 {playerKvp.Key}:");
            foreach (var cooldown in playerKvp.Value)
            {
                sb.AppendLine($"    [{cooldown.skillId}] {cooldown.skillName}: {cooldown.remainingCooldown:F2}s / {cooldown.totalCooldown:F2}s (活跃:{cooldown.isActive})");
            }
        }
        
        sb.AppendLine($"\nBuff列表:");
        foreach (var playerKvp in _playerBuffs)
        {
            sb.AppendLine($"  玩家 {playerKvp.Key}:");
            foreach (var buff in playerKvp.Value)
            {
                sb.AppendLine($"    [{buff.buffId}] {buff.buffName} ({buff.buffType}): {buff.remainingDuration:F2}s / {buff.totalDuration:F2}s, 层数:{buff.stackCount}, 值:{buff.value}");
            }
        }
        
        return sb.ToString();
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[TimerManager] {message}");
        }
    }
    
    #endregion
}
