using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFactoryHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FactoryManager factoryManager;
    //[SerializeField] private IAPManager iapManager;

    [Header("Global UI")]
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI totalProdText;
    [SerializeField] private TextMeshProUGUI boostStatusText;
    [SerializeField] private Button boostButton;
    [SerializeField] private Button buyIapButton;

    [Header("Machines Layout")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject cardPrefab;

    private List<UIMachineCard> spawnedCards = new();

    private void Start()
    {
        factoryManager.OnDataChanged += UpdateUI;
        boostButton.onClick.AddListener(factoryManager.ActivateBoost);
        //buyIapButton.onClick.AddListener(iapManager.PurchaseSmallPack);

        InitCards();
        UpdateUI();
    }

    private void InitCards()
    {
        string json = PlayerPrefs.GetString("FactorySave", string.Empty);
        // Так как конфиг может грузиться асинхронно, берем актуальный список id через менеджер
        var dummyData = JsonUtility.FromJson<PlayerSaveData>(json);

        // Для создания карточек используем данные стейтов, которые инициализировал FactoryManager в Awake
        // Чтобы гарантировать порядок, лучше пройтись по элементам конфига локально
    }

    // Сделаем более надежную инициализацию после Awake менеджера:
    private void DelayedInit()
    {
        foreach (Transform child in cardsContainer) Destroy(child.gameObject);
        spawnedCards.Clear();

        var config = factoryManager.GetMachineConfig("m1"); // Просто проверка доступа к данным
        // Проходимся по всем доступным машинам из конфига менеджера
        // Для демонстрации структуры, предполагаем, что у вас есть явный доступ к ID.
    }

    private void Update()
    {
        // Поминутное/посекундное обновление таймера буста без вызова тяжелого OnDataChanged
        if (factoryManager.IsBoostActive)
        {
            var remaining = factoryManager.BoostEndTime.Value - System.DateTime.UtcNow;
            boostStatusText.text = $"BOOST ACTIVE! X2\nОсталось: {remaining.Seconds}с";
        }
        else
        {
            boostStatusText.text = "Boost выключен";
        }
    }

    private void UpdateUI()
    {
        balanceText.text = $"Баланс: {factoryManager.Balance:F0}";
        totalProdText.text = $"Всего: {factoryManager.CalculateTotalProduction():F1}/сек";

        foreach (var card in spawnedCards)
        {
            card.Refresh();
        }
    }

    private void OnDestroy()
    {
        if (factoryManager != null)
            factoryManager.OnDataChanged -= UpdateUI;
    }
}
