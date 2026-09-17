public interface IIapService
{
    void Initialize(System.Action<string> onPurchaseSuccess, System.Action<string, string> onPurchaseFailed);
    void BuyConsumable(string productId);
}