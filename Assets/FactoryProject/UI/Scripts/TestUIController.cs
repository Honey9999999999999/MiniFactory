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
    [SerializeField] private UIMachineCard machineCardPrefab;
    [SerializeField] private TextMeshProUGUI boostText;

    [Header("Buttons")]
    [SerializeField] private Button boostButton;
    [SerializeField] private Button iapButton;


    private void Start()
    {
        factoryManager.OnDataChanged += UpdateBalance;

        foreach (var machineConfig in factoryManager.Config.Machines)
        {
            CreateMachineCard(machineConfig);
        }

        boostButton.onClick.AddListener(factoryManager.ActivateBoost);

        iapButton.onClick.AddListener(iapManager.PurchaseSmallPack);

        UpdateBalance();
    }

    private void CreateMachineCard(MachineConfig machineConfig)
    {
        var card = Instantiate(machineCardPrefab, machinesList);
        card.Setup(factoryManager, machineConfig.machineId);
    }

    private void Update()
    {
        if (factoryManager.IsBoostActive && factoryManager.boostEndTime.HasValue)
        {
            var timeLeft = factoryManager.boostEndTime.Value - System.DateTime.UtcNow;
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
            $"Всего: {factoryManager.CalculateTotalProduction() * (factoryManager.IsBoostActive ? factoryManager.Config.BoostMultiplier : 1):F1}/сек";
    }

    private void OnDestroy()
    {
        if (factoryManager != null)
            factoryManager.OnDataChanged -= UpdateBalance;
    }
}
