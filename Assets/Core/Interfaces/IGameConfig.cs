using System.Collections.Generic;
using UnityEngine;

public interface IGameConfig
{
    float BoostDuration { get; }
    float BoostMultiplier { get; }
    bool IsBoostEnabled { get; }
    float MaxOfflineProductionTime { get; }
    IReadOnlyList<MachineConfig> Machines { get; }
}
