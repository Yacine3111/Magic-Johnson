using Newtonsoft.Json;

namespace Magik_Johnson.DTOs.Requests;

public class UpdateOrderStatus
{
    [JsonProperty("orderId")]
    public required string OrderId { get; set; }

    [JsonProperty(nameof(NewStatus))]
    public required string NewStatus { get; set; }
}
