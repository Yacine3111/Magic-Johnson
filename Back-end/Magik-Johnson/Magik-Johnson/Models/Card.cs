using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Magik_Johnson.Models;

[Table("Cards")]
public class Card
{
    public Guid Id { get; set; }
    public Guid SetId { get; set; }
    public Guid ScryfallId { get; set; }
    public Guid? DiscountId { get; set; }
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            SortableName = GenerateSortableName(_name);
        }
    }
    public string SortableName { get; private set; } = string.Empty;
    public string? FrenchName { get; set; }
    public string? ManaCost { get; set; }
    public string? Text { get; set; }
    public string? Power { get; set; } //Power peut avoir comme valeur "∞" et "*"
    public string? Toughness { get; set; } //Toughness peut avoir comme valeur "*"
    public string Rarity { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public bool HasFoil { get; set; }
    public bool HasNonFoil { get; set; }
    public decimal Price { get; set; }
    public decimal FoilPrice { get; set; }
    public bool OnlineOnly { get; set; }
    public int ClickCount { get; set; } = default;

    public ICollection<CardType> CardTypes { get; set; } = [];
    public ICollection<CardSubType> CardSubTypes { get; set; } = [];
    public ICollection<Language> Languages { get; set; } = [];
    public ICollection<ArtistCard> ArtistsCards { get; set; } = [];
    public ICollection<Quantity> Quantities { get; set; } = new List<Quantity>();
    public ICollection<CardCart> CardCarts { get; set; } = [];
    public ICollection<CardOrder> CardOrders { get; set; } = [];
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    private static string GenerateSortableName(string name)
    {
        string sortableName = RemoveAccents(name);

        sortableName = Regex.Replace(sortableName, @"[^a-zA-Z0-9 ]", "");

        sortableName = sortableName.Trim();

        if (!sortableName.Any(char.IsLetter))
            return string.Empty;

        return sortableName.ToLower();
    }

    private static string RemoveAccents(string name)
    {
        var normalizedName = name.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (
            var character in normalizedName.Where(c =>
                CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark
            )
        )
        {
            stringBuilder.Append(character);
        }

        return stringBuilder.ToString();
    }
}
