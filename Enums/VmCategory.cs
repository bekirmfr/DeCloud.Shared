using System.Text.Json.Serialization;

namespace DeCloud.Shared.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VmCategory
    {
        Tenant,
        System
    }
}
