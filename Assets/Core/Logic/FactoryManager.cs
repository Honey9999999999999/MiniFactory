using System;
using System.Collections.Generic;
using UnityEngine;

public class FactoryManager : MonoBehaviour
{
    [SerializeField] private GameConfig localConfig;

    private IGameConfig config;
    private PlayerSaveData saveData;

    public double Balance => saveData.balance;
    public DateTime? BoostEndTime { get; private set; }
    public bool IsBoostActive => BoostEndTime.HasValue && DateTime.UtcNow < BoostEndTime.Value;

    public event Action OnDataChanged;

    private void Awake()
    {
        config = localConfig; // Легко заменить на Remote Config внедрением зависимости
        LoadGame();
    }

    private void Update()
    {
        TickProduction(Time.deltaTime);
    }

    private void TickProduction(float deltaTime)
    {
        double totalProd = CalculateTotalProduction();
        double multiplier = IsBoostActive ? config.BoostMultiplier : 1.0;

        saveData.balance += totalProd * multiplier * deltaTime;
        OnDataChanged?.Invoke();
    }

    public double CalculateTotalProduction()
    {
        double total = 0;
        foreach (var state in saveData.machineStates)
        {
            if (state.isUnlocked)
            {
                var cfg = GetMachineConfig(state.machineId);
                total += cfg.baseProduction * Math.Pow(cfg.productionMultiplier, state.level - 1);
            }
        }
        return total;
    }

    public void UnlockMachine(string id)
    {
        var state = saveData.machineStates.Find(s => s.machineId == id);
        var cfg = GetMachineConfig(id);
        if (state != null && !state.isUnlocked && saveData.balance >= cfg.unlockCost)
        {
            saveData.balance -= cfg.unlockCost;
            state.isUnlocked = true;
            OnDataChanged?.Invoke();
        }
    }

    public void UpgradeMachine(string id)
    {
        var state = saveData.machineStates.Find(s => s.machineId == id);
        if (state != null && state.isUnlocked)
        {
            double cost = GetUpgradeCost(id, state.level);
            if (saveData.balance >= cost)
            {
                saveData.balance -= cost;
                state.level++;
                OnDataChanged?.Invoke();
            }
        }
    }

    public void ActivateBoost()
    {
        if (!config.IsBoostEnabled) return;

        if (IsBoostActive)
            BoostEndTime = BoostEndTime.Value.AddSeconds(config.BoostDuration);
        else
            BoostEndTime = DateTime.UtcNow.AddSeconds(config.BoostDuration);

        OnDataChanged?.Invoke();
    }

    public void AddCurrencyFromIAP(double amount)
    {
        saveData.balance += amount;
        OnDataChanged?.Invoke();
    }

    public double GetUpgradeCost(string id, int currentLevel)
    {
        var cfg = GetMachineConfig(id);
        return cfg.baseUpgradeCost * Math.Pow(cfg.upgradeCostMultiplier, currentLevel - 1);
    }

    public MachineConfig GetMachineConfig(string id) =>
        ((List<MachineConfig>)config.Machines).Find(m => m.machineId == id);

    public MachineState GetMachineState(string id) =>
        saveData.machineStates.Find(s => s.machineId == id);

    #region Save/Load & Offline Progress

    private void LoadGame()
    {
        string json = PlayerPrefs.GetString("FactorySave", string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            InitNewGame();
        }
        else
        {
            saveData = JsonUtility.FromJson<PlayerSaveData>(json);
            ProcessOfflineProgress();
        }
    }

    private void InitNewGame()
    {
        saveData = new PlayerSaveData();
        for (int i = 0; i < config.Machines.Count; i++)
        {
            saveData.machineStates.Add(new MachineState
            {
                machineId = config.Machines[i].machineId,
                isUnlocked = (i == 0), // Первая машина открыта сразу
                level = 1
            });
        }
    }

    private void ProcessOfflineProgress()
    {
        if (string.IsNullOrEmpty(saveData.lastSaveTime)) return;

        DateTime lastSave = DateTime.Parse(saveData.lastSaveTime);
        TimeSpan offlineSpan = DateTime.UtcNow - lastSave;

        double totalSeconds = offlineSpan.TotalSeconds;
        if (totalSeconds <= 0) return;

        // Ограничение максимального времени оффлайна
        if (totalSeconds > config.MaxOfflineProductionTime)
            totalSeconds = config.MaxOfflineProductionTime;

        // Расчет буста в оффлайне
        double offlineIncome = 0;
        double totalProd = CalculateTotalProduction();

        if (!string.IsNullOrEmpty(saveData.boostEndTime))
        {
            DateTime boostEnd = DateTime.Parse(saveData.boostEndTime);
            BoostEndTime = boostEnd;

            if (lastSave < boostEnd)
            {
                double remainingBoostSeconds = (boostEnd - lastSave).TotalSeconds;
                double boostSecondsApplied = Math.Min(totalSeconds, remainingBoostSeconds);

                offlineIncome += totalProd * config.BoostMultiplier * boostSecondsApplied;
                totalSeconds -= boostSecondsApplied;
            }
        }

        offlineIncome += totalProd * totalSeconds;
        saveData.balance += offlineIncome;

        Debug.Log($"Оффлайн доход: {offlineIncome:F2} за {offlineSpan.TotalMinutes:F1} мин.");
    }

    private void OnApplicationQuit() => SaveGame();
    private void OnApplicationPause(bool pause) { if (pause) SaveGame(); else LoadGame(); }

    public void SaveGame()
    {
        saveData.lastSaveTime = DateTime.UtcNow.ToString();
        saveData.boostEndTime = BoostEndTime.HasValue ? BoostEndTime.Value.ToString() : string.Empty;
        PlayerPrefs.SetString("FactorySave", JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }
    #endregion
}
