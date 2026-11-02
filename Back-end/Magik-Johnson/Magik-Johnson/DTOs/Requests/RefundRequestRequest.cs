using Newtonsoft.Json;

namespace Magik_Johnson.DTOs.Requests;

public class RefundRequestRequest
{
    [JsonProperty("orderId")]
    public required string OrderId { get; set; }

    [JsonProperty("totalAmount")]
    public double TotalAmount { get; set; }

    [JsonProperty("isPartial")]
    public bool IsPartial { get; set; }

    [JsonProperty("previousOrderStatusId")]
    public required string PreviousOrderStatusId { get; set; }

    [JsonProperty("refundJustification")]
    public required string RefundJustification { get; set; }

    [JsonProperty("comments")]
    public string? Comments { get; set; }

    [JsonProperty("isStillPreparing")]
    public bool? IsStillPreparing { get; set; }

    [JsonProperty("items")]
    public List<RefundItemRequest>? Items { get; set; }
}
