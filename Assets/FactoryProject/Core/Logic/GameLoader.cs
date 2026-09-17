using System;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private const String SAVE_NAME = "FactorySave";

    [SerializeField] private GameConfig localConfig;
    [SerializeField] private FactoryManager factoryManager;

    private IGameConfig config;
    private PlayerSaveData saveData;

    private AnalyticsGameService analyticsService;

    public void Awake()
    {
        config ??= localConfig;
        Initialize();
    }

    private void Initialize()
    {
        analyticsService = new AnalyticsGameService();
        analyticsService.RegisterProvider(new DebugAnalyticsProvider());

        LoadGame();
    }

    private void LoadGame()
    {
        string json = PlayerPrefs.GetString(SAVE_NAME, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            InitNewGame();            
        }
        else
        {
            saveData = JsonUtility.FromJson<PlayerSaveData>(json);            
        }

        factoryManager.Initialize(config, saveData, analyticsService);
        ProcessOfflineProgress();
    }

    private void InitNewGame()
    {
        saveData = new PlayerSaveData();

        for (int i = 0; i < config.Machines.Count; i++)
        {
            saveData.machineStates.Add(new MachineState
            {
                machineId = config.Machines[i].machineId,
                isUnlocked = (i == 0),
                level = 1
            });
        }
    }

    private void ProcessOfflineProgress()
    {
        if (string.IsNullOrEmpty(saveData.lastSaveTime)) return;

        DateTime lastSave = DateTime.Parse(saveData.lastSaveTime);
        TimeSpan offlineSpan = DateTime.UtcNow - lastSave;

        double totalSeconds = Math.Clamp(offlineSpan.TotalSeconds, 0, config.MaxOfflineProductionTime);

        double offlineIncome = 0;
        double totalProd = factoryManager.CalculateTotalProduction();

        if (!string.IsNullOrEmpty(saveData.boostEndTime))
        {
            DateTime boostEnd = DateTime.Parse(saveData.boostEndTime);
            factoryManager.boostEndTime = boostEnd;

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

        analyticsService.LogOfflineIncomeApplied(totalProd, totalSeconds);
    }

    private void OnApplicationQuit() => SaveGame();
    private void OnApplicationPause(bool pause) { if (pause) SaveGame(); else LoadGame(); }

    public void SaveGame()
    {
        saveData.lastSaveTime = DateTime.UtcNow.ToString();
        saveData.balance = factoryManager.Balance;
        saveData.boostEndTime = factoryManager.boostEndTime.HasValue ? factoryManager.boostEndTime.Value.ToString() : string.Empty;
        PlayerPrefs.SetString(SAVE_NAME, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }
}