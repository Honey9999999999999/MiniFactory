using System.Collections.Generic;

public abstract class AnalyticsService
{
    private readonly List<IAnalyticsProvider> _providers = new();

    public void RegisterProvider(IAnalyticsProvider provider)
    {
        _providers.Add(provider);
        provider.Initialize();
    }

    protected void SendEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        foreach (var provider in _providers)
        {
            provider.LogEvent(eventName, parameters);
        }
    }  
}