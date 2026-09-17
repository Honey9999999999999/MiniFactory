using System.Collections.Generic;

public interface IAnalyticsProvider
{
    void Initialize();
    void LogEvent(string eventName, Dictionary<string, object> parameters = null);
}