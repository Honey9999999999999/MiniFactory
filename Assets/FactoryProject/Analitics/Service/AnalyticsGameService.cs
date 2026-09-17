using System.Collections.Generic;

public class AnalyticsGameService : AnalyticsService
{
    public void LogGameStarted() => SendEvent("game_started");

    public void LogMachineUnlocked(string machineId, double unlockCost) =>
        SendEvent("machine_unlocked", new Dictionary<string, object> { { "machine_id", machineId }, { "cost", unlockCost } });

    public void LogMachineUpgraded(string machineId, int newLevel, double upgradeCost) =>
        SendEvent("machine_upgraded", new Dictionary<string, object> { { "machine_id", machineId }, { "level", newLevel }, { "cost", upgradeCost } });

    public void LogBoostStarted(float duration, float multiplier) =>
        SendEvent("boost_started", new Dictionary<string, object> { { "duration", duration }, { "multiplier", multiplier } });

    public void LogOfflineIncomeApplied(double amount, double totalSeconds) =>
        SendEvent("offline_income_applied", new Dictionary<string, object> { { "amount", amount }, { "seconds", totalSeconds } });    
}