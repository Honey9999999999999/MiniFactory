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

    public void Setup(FactoryManager factoryManager, string id)
    {
        manager = factoryManager;
        machineId = id;
        actionButton.onClick.AddListener(OnCardAction);
        Refresh();
    }

    public void Refresh()
    {
        var cfg = manager.GetMachineConfig(machineId);
        var state = manager.GetMachineState(machineId);

        titleText.text = cfg.displayName;

        if (state.isUnlocked)
        {
            double currentProd = cfg.baseProduction * System.Math.Pow(cfg.productionMultiplier, state.level - 1);
            double nextCost = manager.GetUpgradeCost(machineId, state.level);

            infoText.text = $"Ур: {state.level}\nПрод: {currentProd:F1}/с";
            buttonText.text = $"Улучшить\n({nextCost:F0})";
            actionButton.interactable = manager.Balance >= nextCost;
        }
        else
        {
            infoText.text = "ЗАБЛОКИРОВАНО";
            buttonText.text = $"Открыть\n({cfg.unlockCost:F0})";
            actionButton.interactable = manager.Balance >= cfg.unlockCost;
        }
    }

    private void OnCardAction()
    {
        var state = manager.GetMachineState(machineId);
        if (state.isUnlocked)
            manager.UpgradeMachine(machineId);
        else
            manager.UnlockMachine(machineId);
    }
}
