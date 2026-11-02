namespace Magik_Johnson.DTOs.Responses;

public class FilterOptionResponse
{
    public required List<string> Series { get; set; }
    public required List<string> Rarities { get; set; }
    public required List<string> Types { get; set; }
    public required List<string> Languages { get; set; }
    public required string[] Colors { get; set; }
    public required string[] Availabilities { get; set; }
}
