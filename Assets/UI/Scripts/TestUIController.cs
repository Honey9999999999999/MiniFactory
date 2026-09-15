using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestUIController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private FactoryManager factoryManager;
    //[SerializeField] private IAPManager iapManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI globalInfoText;
    [SerializeField] private TextMeshProUGUI boostText;
    [SerializeField] private TextMeshProUGUI machine1Text;
    [SerializeField] private TextMeshProUGUI machine2Text;
    [SerializeField] private TextMeshProUGUI machine3Text;

    [Header("Buttons")]
    [SerializeField] private Button boostButton;
    [SerializeField] private Button machine1Button;
    [SerializeField] private Button machine2Button;
    [SerializeField] private Button machine3Button;
    [SerializeField] private Button iapButton;

    private void Start()
    {
        // Подписываемся на обновление UI при изменении данных
        factoryManager.OnDataChanged += UpdateUI;

        // Вешаем логику на кнопки
        boostButton.onClick.AddListener(factoryManager.ActivateBoost);

        machine1Button.onClick.AddListener(() => {
            var state = factoryManager.GetMachineState("m1");
            if (state.isUnlocked)
                factoryManager.UpgradeMachine("m1");
            else
                factoryManager.UnlockMachine("m1");
        });

        machine2Button.onClick.AddListener(() => {
            var state = factoryManager.GetMachineState("m2");
            if (state.isUnlocked)
                factoryManager.UpgradeMachine("m2");
            else
                factoryManager.UnlockMachine("m2");
        });

        machine3Button.onClick.AddListener(() => {
            var state = factoryManager.GetMachineState("m3");
            if (state.isUnlocked)
                factoryManager.UpgradeMachine("m3");
            else
                factoryManager.UnlockMachine("m3");
        });

        //iapButton.onClick.AddListener(iapManager.PurchaseSmallPack);

        UpdateUI();
    }

    private void Update()
    {
        // Кадровая подкраска таймера буста
        if (factoryManager.IsBoostActive && factoryManager.BoostEndTime.HasValue)
        {
            var timeLeft = factoryManager.BoostEndTime.Value - System.DateTime.UtcNow;
            boostText.text = $"🔥 БУСТ АКТИВЕН! Осталось: {timeLeft.Seconds} сек (x2)";
        }
        else
        {
            boostText.text = "Буст не активен";
        }
    }

    private void UpdateUI()
    {
        // 1. Общая инфа
        globalInfoText.text = $"Баланс: {factoryManager.Balance:F1} | Всего: {factoryManager.CalculateTotalProduction():F1}/сек";

        // 2. Инфа по Машине 1 (открыта всегда)
        var m1State = factoryManager.GetMachineState("m1");
        var m1Cfg = factoryManager.GetMachineConfig("m1");
        double m1Cost = factoryManager.GetUpgradeCost("m1", m1State.level);
        machine1Text.text = $"{m1Cfg.displayName}: Ур. {m1State.level} (Апгрейд: {m1Cost:F0})";

        // 3. Инфа по Машине 2 (блокирована по умолчанию)
        var m2State = factoryManager.GetMachineState("m2");
        var m2Cfg = factoryManager.GetMachineConfig("m2");

        if (m2State.isUnlocked)
        {
            double m2Cost = factoryManager.GetUpgradeCost("m2", m2State.level);
            machine2Text.text = $"{m2Cfg.displayName}: Ур. {m2State.level} (Апгрейд: {m2Cost:F0})";
            machine2Button.GetComponentInChildren<TextMeshProUGUI>().text = "Улучшить М2";
        }
        else
        {
            machine2Text.text = $"{m2Cfg.displayName}: [ЗАБЛОКИРОВАНО] (Цена: {m2Cfg.unlockCost})";
            machine2Button.GetComponentInChildren<TextMeshProUGUI>().text = "Купить М2";
        }

        // 3. Инфа по Машине 2 (блокирована по умолчанию)
        var m3State = factoryManager.GetMachineState("m3");
        var m3Cfg = factoryManager.GetMachineConfig("m3");

        if (m3State.isUnlocked)
        {
            double m3Cost = factoryManager.GetUpgradeCost("m3", m3State.level);
            machine3Text.text = $"{m3Cfg.displayName}: Ур. {m3State.level} (Апгрейд: {m3Cost:F0})";
            machine3Button.GetComponentInChildren<TextMeshProUGUI>().text = "Улучшить М3";
        }
        else
        {
            machine3Text.text = $"{m3Cfg.displayName}: [ЗАБЛОКИРОВАНО] (Цена: {m3Cfg.unlockCost})";
            machine3Button.GetComponentInChildren<TextMeshProUGUI>().text = "Купить М3";
        }
    }

    private void OnDestroy()
    {
        if (factoryManager != null)
            factoryManager.OnDataChanged -= UpdateUI;
    }
}
