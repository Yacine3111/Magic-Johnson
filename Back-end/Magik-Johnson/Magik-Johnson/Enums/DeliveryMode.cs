using System.Text.Json.Serialization;

namespace Magik_Johnson.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeliveryMode
{
    InStorePickup,
    HomeDelivery,
}
