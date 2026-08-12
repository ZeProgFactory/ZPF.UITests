using System.Text.Json.Serialization;

namespace DevFlowMonitor;

public class DevFlowAgent
{
   [JsonPropertyName("id")]
   public string Id { get; set; } = string.Empty;

   [JsonPropertyName("project")]
   public string Project { get; set; } = string.Empty;

   [JsonPropertyName("tfm")]
   public string Tfm { get; set; } = string.Empty;

   [JsonPropertyName("platform")]
   public string Platform { get; set; } = string.Empty;

   [JsonPropertyName("appName")]
   public string AppName { get; set; } = string.Empty;

   [JsonPropertyName("port")]
   public int Port { get; set; }

   [JsonPropertyName("version")]
   public string Version { get; set; } = string.Empty;

   [JsonPropertyName("sessionId")]
   public string SessionId { get; set; } = string.Empty;

   [JsonPropertyName("connectedAt")]
   public DateTimeOffset ConnectedAt { get; set; }
}

[JsonSerializable(typeof(List<DevFlowAgent>))]
[JsonSerializable(typeof(List<TreeNode>))]
[JsonSerializable(typeof(List<ElementInfo>))]
[JsonSerializable(typeof(TapRequest))]
[JsonSerializable(typeof(ActionResponse))]
internal partial class DevFlowJsonContext : JsonSerializerContext { }
