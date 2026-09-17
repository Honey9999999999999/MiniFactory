using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DebugAnalyticsProvider : IAnalyticsProvider
{
    public void Initialize()
    {
        Debug.Log("[Analytics] Debug Provider инициализирован.");
    }

    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        string paramsLog = "{}";

        if (parameters != null && parameters.Count > 0)
        {
            var sb = new StringBuilder();
            sb.Append("{ ");

            List<string> pairs = new List<string>();
            foreach (var kvp in parameters)
            {
                string valueStr = kvp.Value is double d ? d.ToString("F2") :
                                 kvp.Value is float f ? f.ToString("F2") :
                                 kvp.Value?.ToString() ?? "null";

                pairs.Add($"\"{kvp.Key}\": {valueStr}");
            }

            sb.Append(string.Join(", ", pairs));
            sb.Append(" }");
            paramsLog = sb.ToString();
        }

        Debug.Log($"<color=#00FFCC>[Analytics Event]</color> <b>{eventName}</b> | Params: {paramsLog}");
    }
}
