using System.Text.Json;
using LocallyGDriveApi.Shared.Utils;
using Serilog.Events;
using Serilog.Formatting;


namespace LocallyGDriveApi.Logging;

public class SimpleJsonFormatter : ITextFormatter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var payload = new Dictionary<string, object?>
        {
            ["timestamp"] = logEvent.Timestamp.ToUniversalTime().ToString("O"),
            ["level"] = LogUtil.SimplifyLogLevel(logEvent.Level),
            ["message"] = logEvent.RenderMessage()
        };

        AddIfPresent(logEvent, payload, "correlationId");
        AddIfPresent(logEvent, payload, "method");
        AddIfPresent(logEvent, payload, "path");

        if (logEvent.Exception != null)
        {
            payload["exception"] = new Dictionary<string, object?>
            {
                ["exception"] = logEvent.Exception.ToString(),
                ["exceptionType"] = logEvent.Exception.GetType().FullName,
                ["exceptionMessage"] = logEvent.Exception.Message,
                ["stackTrace"] = logEvent.Exception.StackTrace
            };
        }

        foreach (var property in logEvent.Properties)
        {
            var key = ToCamelCase(property.Key);
            if (payload.ContainsKey(key))
            {
                continue;
            }

            payload[key] = Simplify(property.Value);
        }

        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        output.WriteLine(json);
    }

    private static void AddIfPresent(LogEvent logEvent, IDictionary<string, object?> payload, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var value))
        {
            payload[propertyName] = Simplify(value);
        }
    }

    private static object? Simplify(LogEventPropertyValue value) => value switch
    {
        ScalarValue scalar => scalar.Value,
        SequenceValue sequence => sequence.Elements.Select(Simplify).ToArray(),
        StructureValue structure => structure.Properties.ToDictionary(p => ToCamelCase(p.Name), p => Simplify(p.Value)),
        DictionaryValue dictionary => dictionary.Elements.ToDictionary(kvp => kvp.Key.Value?.ToString() ?? string.Empty, kvp => Simplify(kvp.Value)),
        _ => value.ToString()
    };

    private static string ToCamelCase(string value) =>
        string.IsNullOrEmpty(value) || char.IsLower(value[0])
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
}