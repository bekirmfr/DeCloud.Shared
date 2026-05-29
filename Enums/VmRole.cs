using System.Text.Json.Serialization;

namespace DeCloud.Shared.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VmRole
    {
        General,
        Relay,
        Dht,
        Inference,
        BlockStore
    }
}
