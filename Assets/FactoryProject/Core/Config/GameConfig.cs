using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Factory/GameConfig")]
public class GameConfig : ScriptableObject, IGameConfig
{
    [SerializeField] private float boostDuration = 30f;
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private bool isBoostEnabled = true;
    [SerializeField] private float maxOfflineProductionTime = 7200f;
    [SerializeField] private List<MachineConfig> machines;

    public float BoostDuration => boostDuration;
    public float BoostMultiplier => boostMultiplier;
    public bool IsBoostEnabled => isBoostEnabled;
    public float MaxOfflineProductionTime => maxOfflineProductionTime;
    public IReadOnlyList<MachineConfig> Machines => machines;
}
