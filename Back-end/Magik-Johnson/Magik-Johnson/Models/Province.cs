using Newtonsoft.Json;

namespace Magik_Johnson.Models;

public class Province
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonProperty("name")]
    public required string Name { get; set; }
    public Guid CountryId { get; set; }

    [JsonIgnore]
    public Country Country { get; set; }
}
