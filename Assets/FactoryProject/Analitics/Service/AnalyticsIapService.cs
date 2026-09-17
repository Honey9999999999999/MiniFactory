using System.Collections.Generic;

public class AnalyticsIapService : AnalyticsService
{
    public void LogIapConnecting() => SendEvent("iap_connecting..");
    public void LogIapLoadingProducts() => SendEvent("iap_loading_products..");

    public void LogIapInitialized() => SendEvent("iap_initialized");

    public void LogIapPurchasePending(string transactionId) =>
        SendEvent("iap_purchase_pending", new Dictionary<string, object> { { "transaction_id", transactionId } });

    public void LogIapCartEmpty() => SendEvent("iap_cart_empty");

    public void LogIapProductReceived(string productId) =>
        SendEvent("iap_product_received", new Dictionary<string, object> { { "product_id", productId } });

    public void LogPurchaseSucceeded(string productId) =>
        SendEvent("purchase_succeeded", new Dictionary<string, object> { { "product_id", productId } });

    public void LogPurchaseFailed(string productId, string reason) =>
        SendEvent("purchase_failed", new Dictionary<string, object> { { "product_id", productId }, { "reason", reason } });

    public void LogIapUnavailable(string reason) =>
        SendEvent("iap_unavailable", new Dictionary<string, object> { { "reason", reason } });
}
