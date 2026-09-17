using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMachineCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private FactoryManager manager;
    private string machineId;

    private MachineConfig config;
    private MachineState state;

    public void Setup(FactoryManager factoryManager, string id)
    {
        manager = factoryManager;
        machineId = id;
        config = manager.MachineDataMap[id].config;
        state = manager.MachineDataMap[id].state;
        actionButton.onClick.AddListener(OnCardAction);
        factoryManager.OnDataChanged += () => Refresh();
    }

    public void Refresh()
    {
        titleText.text = config.displayName;

        if (state.isUnlocked)
        {
            double currentProd = config.baseProduction * System.Math.Pow(config.productionMultiplier, state.level - 1);
            double nextCost = manager.GetUpgradeCost(machineId, state.level);

            infoText.text = $"Ур: {state.level}\nПрод: {currentProd:F1}/с";
            buttonText.text = $"Улучшить\n({nextCost:F0})";
            actionButton.interactable = manager.Balance >= nextCost;
        }
        else
        {
            infoText.text = "ЗАБЛОКИРОВАНО";
            buttonText.text = $"Открыть\n({config.unlockCost:F0})";
            actionButton.interactable = manager.Balance >= config.unlockCost;
        }
    }

    private void OnCardAction()
    {
        if (state.isUnlocked)
            manager.UpgradeMachine(machineId);
        else
            manager.UnlockMachine(machineId);
    }
}
