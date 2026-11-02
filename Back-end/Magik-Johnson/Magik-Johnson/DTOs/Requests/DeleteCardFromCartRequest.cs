namespace Magik_Johnson.DTOs.Requests;

public class DeleteCardFromCartRequest
{
    public string LanguageName { get; set; } = default!;
    public bool IsFoil { get; set; }
}
