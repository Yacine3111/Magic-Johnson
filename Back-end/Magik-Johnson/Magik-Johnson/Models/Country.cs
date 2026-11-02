using Newtonsoft.Json;

namespace Magik_Johnson.Models;

public class Country
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("states")]
    public List<Province> Provinces { get; set; } = new List<Province>();
}
