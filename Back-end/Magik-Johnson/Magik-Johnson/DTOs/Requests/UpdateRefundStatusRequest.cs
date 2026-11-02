using System.Text.Json.Serialization;

namespace Magik_Johnson.DTOs.Requests;

public class UpdateRefundStatusRequest
{
    [JsonPropertyName("refundRequestId")]
    public required string RefundRequestId { get; set; }

    [JsonPropertyName("newRefundStatus")]
    public required string NewRefundRequestStatus { get; set; }
}
