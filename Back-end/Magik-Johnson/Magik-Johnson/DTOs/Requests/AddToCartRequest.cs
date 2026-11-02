namespace Magik_Johnson.DTOs.Requests;

public class AddToCartRequest
{
    public required List<CartItemRequest> Items { get; set; }
}
