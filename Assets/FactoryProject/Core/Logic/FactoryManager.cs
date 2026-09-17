using System;
using System.Collections.Generic;
using UnityEngine;

public class FactoryManager : MonoBehaviour
{
    public class MachineData
    {
        public readonly MachineConfig config;
        public readonly MachineState state;

        public MachineData(MachineConfig config, MachineState state)
        {
            this.config = config;
            this.state = state;
        }
    }



    public event Action OnDataChanged;

    public IGameConfig Config => config;
    private IGameConfig config;
    private PlayerSaveData saveData;

    private AnalyticsGameService analyticsService;

    public Dictionary<string, MachineData> MachineDataMap { get; private set; }

    public double Balance => saveData.balance;

    public DateTime? boostEndTime;
    public bool IsBoostActive => boostEndTime.HasValue && DateTime.UtcNow < boostEndTime.Value;
    


    public void Initialize(IGameConfig config, PlayerSaveData saveData, AnalyticsGameService analyticsService)
    {
        this.config = config;
        this.saveData = saveData;
        this.analyticsService = analyticsService;        

        MachineDataMap = new Dictionary<string, MachineData>();
        foreach (var machineConfig in config.Machines)
        {
            MachineDataMap.Add(machineConfig.machineId, new MachineData(
                machineConfig,
                saveData.machineStates.Find(m => m.machineId == machineConfig.machineId)
            ));
        }

        analyticsService.LogGameStarted();
    }

    private void Update()
    {
        TickProduction(Time.deltaTime);
    }

    private void TickProduction(float deltaTime)
    {
        double totalProd = CalculateTotalProduction();
        double multiplier = IsBoostActive ? config.BoostMultiplier : 1;

        saveData.balance += totalProd * multiplier * deltaTime;
        OnDataChanged?.Invoke();
    }

    public double CalculateTotalProduction()
    {
        double total = 0;
        foreach (var machineData in MachineDataMap.Values)
        {
            if (machineData.state.isUnlocked)
            {
                total += machineData.config.baseProduction 
                    * Math.Pow(machineData.config.productionMultiplier, machineData.state.level - 1);
            }
        }
        return total;
    }

    public void UnlockMachine(string id)
    {
        var state = MachineDataMap[id].state;
        var cfg = MachineDataMap[id].config;
        if (state != null && !state.isUnlocked && saveData.balance >= cfg.unlockCost)
        {
            saveData.balance -= cfg.unlockCost;
            state.isUnlocked = true;

            analyticsService.LogMachineUnlocked(id, cfg.unlockCost);
        }
    }

    public void UpgradeMachine(string id)
    {
        var state = MachineDataMap[id].state;
        if (state != null && state.isUnlocked)
        {
            double cost = GetUpgradeCost(id, state.level);
            if (saveData.balance >= cost)
            {
                saveData.balance -= cost;
                state.level++;

                analyticsService.LogMachineUpgraded(id, state.level, cost);
            }
        }
    }

    public void ActivateBoost()
    {
        if (!config.IsBoostEnabled || IsBoostActive) return;

        boostEndTime = DateTime.UtcNow.AddSeconds(config.BoostDuration);

        analyticsService.LogBoostStarted(config.BoostDuration, config.BoostMultiplier);
    }

    public void AddCurrencyFromIAP(double amount)
    {
        saveData.balance = Math.Max(saveData.balance + amount, 0);
    }

    public double GetUpgradeCost(string id, int currentLevel)
    {
        var cfg = MachineDataMap[id].config;
        return cfg.baseUpgradeCost * Math.Pow(cfg.upgradeCostMultiplier, currentLevel - 1);
    }
}
