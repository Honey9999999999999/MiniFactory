using System;

[Serializable]
public class MachineConfig
{
    public string machineId;
    public string displayName;
    public double baseProduction;
    public double baseUpgradeCost;
    public double unlockCost;
    public float upgradeCostMultiplier = 1.15f;
    public float productionMultiplier = 1.1f;
}
