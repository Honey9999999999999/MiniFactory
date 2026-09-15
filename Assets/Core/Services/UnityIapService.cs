//using UnityEngine;
//using UnityEngine.Purchasing;
//using System;

//public class UnityIapService : IIapService
//{
//    // Использован StoreController вместо устаревшего IStoreController
//    private StoreController storeController;

//    private Action<string> purchaseSuccessCallback;
//    private Action<string, string> purchaseFailedCallback;

//    public const string ProductCoinsPackSmall = "coins_pack_small";

//    public async void Initialize(Action<string> onPurchaseSuccess, Action<string, string> onPurchaseFailed)
//    {
//        this.purchaseSuccessCallback = onPurchaseSuccess;
//        this.purchaseFailedCallback = onPurchaseFailed;

//        try
//        {
//            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
//            builder.AddProduct(ProductCoinsPackSmall, ProductType.Consumable);

//            // Асинхронное подключение возвращает актуальный StoreController в IAP v5
//            storeController = await UnityIAPServices.Connect(builder);

//            // Подписка на новые жизненные циклы заказов
//            storeController.OnOrderConfirmed += OnOrderConfirmed;
//            storeController.OnPurchaseFailed += OnPurchaseOrderFailed;

//            Debug.Log("Unity IAP v5 успешно инициализирован асинхронно.");
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"Ошибка инициализации Unity IAP v5: {ex.Message}");
//            purchaseFailedCallback?.Invoke("all", $"Init Failed: {ex.Message}");
//        }
//    }

//    public void BuyConsumable(string productId)
//    {
//        if (storeController != null)
//        {
//            // Запуск покупки через метод нового StoreController
//            storeController.PurchaseProduct(productId);
//        }
//        else
//        {
//            purchaseFailedCallback?.Invoke(productId, "IAP Service не инициализирован.");
//        }
//    }

//    private void OnOrderConfirmed(ConfirmedOrder order)
//    {
//        string id = order.productId;
//        purchaseSuccessCallback?.Invoke(id);

//        // Подтверждаем транзакцию для завершения цикла на стороне платформы
//        order.Confirm();
//    }

//    private void OnPurchaseOrderFailed(FailedOrder failedOrder)
//    {
//        string id = failedOrder.productId;
//        string reasonDescription = failedOrder.failureDescription.message;
//        purchaseFailedCallback?.Invoke(id, reasonDescription);
//    }
//}