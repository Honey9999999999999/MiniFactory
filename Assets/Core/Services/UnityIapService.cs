using System;
using System.Linq;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;

public class UnityIapService : IIapService
{
    private StoreController storeController;

    private Action<string> purchaseSuccessCallback;
    private Action<string, string> purchaseFailedCallback;

    public const string ProductCoinsPackSmall = "coins_pack_small";

    public async void Initialize(Action<string> onPurchaseSuccess, Action<string, string> onPurchaseFailed)
    {
        this.purchaseSuccessCallback = onPurchaseSuccess;
        this.purchaseFailedCallback = onPurchaseFailed;

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            CatalogProvider catalogProvider = new CatalogProvider();
            catalogProvider.AddProduct(ProductCoinsPackSmall, ProductType.Consumable);

            var products = catalogProvider.GetProducts();

            storeController = UnityIAPServices.StoreController();

            Debug.Log("Подключение к магазину...");
            await storeController.Connect();


            Debug.Log("Загрузка продуктов...");
            storeController.FetchProducts(products);

            storeController.OnPurchasePending += HandlePurchasePending;
            storeController.OnPurchasesFetched += HandlePurchasesFetched;

            Debug.Log("Unity IAP инициализирован.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка инициализации Unity IAP: {ex.Message}");
            purchaseFailedCallback?.Invoke("all", $"Init Failed: {ex.Message}");
        }
    }

    private void HandlePurchasePending(PendingOrder order)
    {
        var transactionId = order.Info.TransactionID;
        Debug.Log($"[IAP] Покупка в процессе. ID Транзакции: {transactionId}");

        var cartItem = order.CartOrdered.Items().FirstOrDefault();
        if (cartItem == null)
        {
            Debug.LogError("[IAP] Ошибка: Корзина заказа пуста!");
            return;
        }

        purchaseSuccessCallback.Invoke(cartItem.Product.definition.id);

        storeController.ConfirmPurchase(order);
    }

    private void HandlePurchasesFetched(Orders orders)
    {
        foreach (var order in orders.ConfirmedOrders)
        {
            order.Info.PurchasedProductInfo.ForEach(o => Debug.Log($"Найдена существующая покупка: {o.productId}"));

            // По идеи логика восстановления .-.
        }
    }

    public void BuyConsumable(string productId)
    {
        if (storeController != null)
        {
            storeController.PurchaseProduct(productId);
        }
        else
        {
            purchaseFailedCallback?.Invoke(productId, "IAP Service не инициализирован.");
        }
    }
}