using UnityEngine;

public class IAPManager : MonoBehaviour
{
    [SerializeField] private FactoryManager factoryManager;
    [SerializeField] private double coinsPackReward = 5000;

    private IIapService iapService;

    private void Start()
    {
        iapService = new UnityIapService();
        iapService.Initialize(OnProductPurchased, OnProductFailed);
    }

    public void PurchaseSmallPack()
    {
        iapService.BuyConsumable(UnityIapService.ProductCoinsPackSmall);
    }

    private void OnProductPurchased(string productId)
    {
        if (productId == UnityIapService.ProductCoinsPackSmall)
        {
            factoryManager.AddCurrencyFromIAP(coinsPackReward);
            Debug.Log("Покупка успешна! Валюта начислена.");
        }
    }

    private void OnProductFailed(string productId, string reason)
    {
        Debug.LogError($"Ошибка покупки {productId}: {reason}");
        // Здесь можно вызвать UI-нотификацию для игрока
    }
}
