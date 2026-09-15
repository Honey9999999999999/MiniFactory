using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachineCard : MonoBehaviour
{
    public TextMeshProUGUI machineState => _machineState;
    [SerializeField] private TextMeshProUGUI _machineState;

    public Button machineButton => _machineButton;
    [SerializeField] private Button _machineButton;
}
