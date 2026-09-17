using System;
using System.Linq;
using Unity.Services.Core;
using UnityEngine.Purchasing;

public class UnityIapService : IIapService
{
    private StoreController storeController;

    private AnalyticsIapService analyticsService;

    private Action<string> purchaseSuccessCallback;
    private Action<string, string> purchaseFailedCallback;

    public const string ProductCoinsPackSmall = "coins_pack_small";

    public async void Initialize(Action<string> onPurchaseSuccess, Action<string, string> onPurchaseFailed)
    {
        analyticsService = new AnalyticsIapService();
        analyticsService.RegisterProvider(new DebugAnalyticsProvider());

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

            analyticsService.LogIapConnecting();

            await storeController.Connect();


            analyticsService.LogIapLoadingProducts();
            storeController.FetchProducts(products);

            storeController.OnPurchasePending += HandlePurchasePending;
            storeController.OnPurchasesFetched += HandlePurchasesFetched;

            analyticsService.LogIapInitialized();
        }
        catch (Exception ex)
        {
            analyticsService.LogIapUnavailable("Init failed");
            purchaseFailedCallback?.Invoke("all", $"Init Failed: {ex.Message}");
        }
    }

    private void HandlePurchasePending(PendingOrder order)
    {
        var transactionId = order.Info.TransactionID;
        analyticsService.LogIapPurchasePending(transactionId);

        var cartItem = order.CartOrdered.Items().FirstOrDefault();
        if (cartItem == null)
        {
            analyticsService.LogIapCartEmpty();
            return;
        }

        purchaseSuccessCallback.Invoke(cartItem.Product.definition.id);

        storeController.ConfirmPurchase(order);
    }

    private void HandlePurchasesFetched(Orders orders)
    {
        foreach (var order in orders.ConfirmedOrders)
        {
            order.Info.PurchasedProductInfo.ForEach(o => analyticsService.LogIapProductReceived(o.productId));

            // По идеи логика восстановления .-.
        }
    }

    public void BuyConsumable(string productId)
    {
        if (storeController != null)
        {
            storeController.PurchaseProduct(productId);
            analyticsService.LogPurchaseSucceeded(productId);
        }
        else
        {
            analyticsService.LogPurchaseFailed(productId, "IAP is not initialized.");
            purchaseFailedCallback?.Invoke(productId, "IAP is not initialized.");
        }
    }
}