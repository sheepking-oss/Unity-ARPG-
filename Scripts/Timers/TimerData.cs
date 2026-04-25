using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CooldownData
{
    public int skillId;
    public string skillName;
    public float totalCooldown;
    public float remainingCooldown;
    public float startTime;
    public bool isActive;
    public int ownerId;
    
    public CooldownData(int skillId, string skillName, float totalCooldown, int ownerId = 0)
    {
        this.skillId = skillId;
        this.skillName = skillName;
        this.totalCooldown = totalCooldown;
        this.remainingCooldown = totalCooldown;
        this.startTime = Time.unscaledTime;
        this.isActive = true;
        this.ownerId = ownerId;
    }
    
    public float GetProgress()
    {
        if (totalCooldown <= 0) return 1f;
        return 1f - (remainingCooldown / totalCooldown);
    }
    
    public bool IsReady()
    {
        return remainingCooldown <= 0f;
    }
}

[System.Serializable]
public class BuffData
{
    public int buffId;
    public string buffName;
    public ModifierType buffType;
    public float totalDuration;
    public float remainingDuration;
    public float startTime;
    public float value;
    public bool isActive;
    public int stackCount;
    public int maxStacks;
    public int ownerId;
    
    public BuffData(int buffId, string buffName, ModifierType buffType, float totalDuration, float value, int maxStacks = 1, int ownerId = 0)
    {
        this.buffId = buffId;
        this.buffName = buffName;
        this.buffType = buffType;
        this.totalDuration = totalDuration;
        this.remainingDuration = totalDuration;
        this.startTime = Time.unscaledTime;
        this.value = value;
        this.isActive = true;
        this.stackCount = 1;
        this.maxStacks = maxStacks;
        this.ownerId = ownerId;
    }
    
    public float GetProgress()
    {
        if (totalDuration <= 0) return 0f;
        return remainingDuration / totalDuration;
    }
    
    public bool IsExpired()
    {
        return remainingDuration <= 0f;
    }
    
    public void Refresh()
    {
        remainingDuration = totalDuration;
        startTime = Time.unscaledTime;
    }
    
    public bool AddStack()
    {
        if (stackCount < maxStacks)
        {
            stackCount++;
            Refresh();
            return true;
        }
        Refresh();
        return false;
    }
}

[System.Serializable]
public class TimerSnapshot
{
    public List<CooldownSnapshot> cooldownSnapshots;
    public List<BuffSnapshot> buffSnapshots;
    public float snapshotTime;
    
    public TimerSnapshot()
    {
        cooldownSnapshots = new List<CooldownSnapshot>();
        buffSnapshots = new List<BuffSnapshot>();
        snapshotTime = Time.unscaledTime;
    }
}

[System.Serializable]
public class CooldownSnapshot
{
    public int skillId;
    public string skillName;
    public float totalCooldown;
    public float remainingCooldown;
    public int ownerId;
}

[System.Serializable]
public class BuffSnapshot
{
    public int buffId;
    public string buffName;
    public ModifierType buffType;
    public float totalDuration;
    public float remainingDuration;
    public float value;
    public int stackCount;
    public int maxStacks;
    public int ownerId;
}
