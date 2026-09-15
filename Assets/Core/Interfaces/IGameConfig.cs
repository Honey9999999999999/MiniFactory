using System.Collections.Generic;

public interface IGameConfig
{
    float BoostDuration { get; }

    float BoostMultiplier { get; }

    bool IsBoostEnabled { get; }

    float MaxOfflineProductionTime { get; }

    IReadOnlyList<MachineConfig> Machines { get; }
}
