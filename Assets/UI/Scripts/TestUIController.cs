using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestUIController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private FactoryManager factoryManager;
    [SerializeField] private IAPManager iapManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI globalInfoText;
    [SerializeField] private Transform machinesList;
    [SerializeField] private MachineCard machineCardPrefab;
    [SerializeField] private TextMeshProUGUI boostText;

    [Header("Buttons")]
    [SerializeField] private Button boostButton;
    [SerializeField] private Button iapButton;

    private Dictionary<string, MachineCard> machineCardMap;

    private void Start()
    {
        factoryManager.OnDataChanged += UpdateBalance;

        machineCardMap = new Dictionary<string, MachineCard>();
        foreach (var machineConfig in factoryManager.config.Machines)
        {
            machineCardMap.Add(machineConfig.machineId, CreateMachineCard(machineConfig));
            UpdateMachineUI(machineConfig);
        }

        boostButton.onClick.AddListener(factoryManager.ActivateBoost);

        iapButton.onClick.AddListener(iapManager.PurchaseSmallPack);

        UpdateBalance();
    }

    private MachineCard CreateMachineCard(MachineConfig machineConfig)
    {
        var machineId = machineConfig.machineId;
        var card = Instantiate(machineCardPrefab, machinesList);

        card.machineButton.onClick.AddListener(() =>
        {
            var state = factoryManager.GetMachineState(machineId);
            if (state.isUnlocked)
                factoryManager.UpgradeMachine(machineId);
            else
                factoryManager.UnlockMachine(machineId);
        });

        card.machineButton.onClick.AddListener(() => UpdateMachineUI(machineConfig));

        return card;
    }

    private void Update()
    {
        if (factoryManager.IsBoostActive && factoryManager.BoostEndTime.HasValue)
        {
            var timeLeft = factoryManager.BoostEndTime.Value - System.DateTime.UtcNow;
            boostText.text = $"БУСТ АКТИВЕН! Осталось: {timeLeft.Seconds} сек (x2)";
        }
        else
        {
            boostText.text = "Буст не активен";
        }
    }

    private void UpdateBalance()
    {
        globalInfoText.text =
            $"Баланс: {factoryManager.Balance:F1}\n" +
            $"Всего: {factoryManager.CalculateTotalProduction() * (factoryManager.IsBoostActive ? factoryManager.config.BoostMultiplier : 1):F1}/сек";
    }

    private void UpdateMachineUI(MachineConfig machineConfig)
    {
        var machineId = machineConfig.machineId;
        var machineState = factoryManager.GetMachineState(machineId);

        if (machineState.isUnlocked)
        {
            double machineCost = factoryManager.GetUpgradeCost(machineId, machineState.level);
            machineCardMap[machineId].machineState.text = $"{machineConfig.displayName}: Ур. {machineState.level} (Апгрейд: {machineCost:F0})";
            machineCardMap[machineId].machineButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Улучшить {machineId.ToUpper()}";
        }
        else
        {
            machineCardMap[machineId].machineState.text = $"{machineConfig.displayName}: [ЗАБЛОКИРОВАНО] (Цена: {machineConfig.unlockCost})";
            machineCardMap[machineId].machineButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Купить {machineId.ToUpper()}";
        }
    }

    private void OnDestroy()
    {
        if (factoryManager != null)
            factoryManager.OnDataChanged -= UpdateBalance;
    }
}
