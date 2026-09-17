using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class FactoryManagerTest
{
    private class MockMachineConfig : MachineConfig
    {
        public MockMachineConfig(string id, double prod, double upgradeCost, double unlock, float costMult, float prodMult)
        {
            machineId = id;
            displayName = "Test Machine";
            baseProduction = prod;
            baseUpgradeCost = upgradeCost;
            unlockCost = unlock;
            upgradeCostMultiplier = costMult;
            productionMultiplier = prodMult;
        }
    }

    private class MockGameConfig : IGameConfig
    {
        public float BoostDuration => 10f;
        public float BoostMultiplier => 2f;
        public bool IsBoostEnabled => true;
        public float MaxOfflineProductionTime => 7200f;
        public IReadOnlyList<MachineConfig> Machines { get; set; }
    }

    private FactoryManager CreateTestFactory(out MockGameConfig config)
    {
        PlayerPrefs.DeleteAll();

        var gameObject = new GameObject();
        var gameLoader = gameObject.AddComponent<GameLoader>();
        var factory = gameObject.AddComponent<FactoryManager>();

        config = new MockGameConfig
        {
            Machines = new List<MachineConfig>
            {
                new MockMachineConfig("m1", 10.0, 100.0, 0.0, 1.5f, 2.0f),
                new MockMachineConfig("m2", 50.0, 500.0, 1000.0, 1.5f, 2.0f)
            }
        };

        var loaderType = gameLoader.GetType();

        loaderType.GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(gameLoader, config);

        loaderType.GetField("factoryManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(gameLoader, factory);

        factory.GetType().GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(factory, config);

        var initMethod = loaderType.GetMethod("Initialize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

        if (initMethod != null)
        {
            initMethod.Invoke(gameLoader, null);
        }
        else
        {
            var awakeMethod = loaderType.GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            awakeMethod?.Invoke(gameLoader, null);
        }

        return factory;
    }

    [Test]
    public void CalculateTotalProduction_OnlyFirstMachineActive_ReturnsBaseProduction()
    {
        var factory = CreateTestFactory(out var config);

        double totalProduction = factory.CalculateTotalProduction();

        Assert.AreEqual(10.0, totalProduction, "Начальное производство первой машины рассчитано неверно.");
    }

    [Test]
    public void UnlockMachine_InsufficientBalance_MachineStaysLocked()
    {
        var factory = CreateTestFactory(out var config);

        factory.UnlockMachine("m2");
        var machineState = factory.MachineDataMap["m2"].state;

        Assert.IsFalse(machineState.isUnlocked, "Машина разблокировалась, несмотря на нехватку средств на балансе.");
    }

    [Test]
    public void GetUpgradeCost_LevelIncreases_CostMultipliesCorrectly()
    {
        var factory = CreateTestFactory(out var config);

        double costLevel1 = factory.GetUpgradeCost("m1", 1); // 100 * (1.5 ^ 0) = 100
        double costLevel2 = factory.GetUpgradeCost("m1", 2); // 100 * (1.5 ^ 1) = 150
        double costLevel3 = factory.GetUpgradeCost("m1", 3); // 100 * (1.5 ^ 2) = 225

        Assert.AreEqual(100.0, costLevel1);
        Assert.AreEqual(150.0, costLevel2);
        Assert.AreEqual(225.0, costLevel3);
    }
}
