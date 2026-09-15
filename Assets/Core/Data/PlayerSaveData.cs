using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public double balance;
    public string lastSaveTime;
    public string boostEndTime;
    public List<MachineState> machineStates = new();
}
