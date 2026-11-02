using System.IO.Compression;
using System.Net;
using Magik_Johnson.Data;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using Type = Magik_Johnson.Models.Type;

namespace Magik_Johnson.Services;

public class DbDataInsertion : IDbDataInsertion
{
    private readonly ApplicationDbContext _applicationDbContext;

    public DbDataInsertion(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await InsertDataAsync();
    }

    public async Task InsertDataAsync()
    {
        using HttpClient httpClient = new() { Timeout = TimeSpan.FromMinutes(30) };

        try
        {
            await using var zipAllPrintingsStream = await httpClient.GetStreamAsync(
                "https://mtgjson.com/api/v5/AllPrintings.json.zip"
            );

            using ZipArchive zipArchive = new(zipAllPrintingsStream, ZipArchiveMode.Read);
            var entry =
                zipArchive.GetEntry("AllPrintings.json")
                ?? throw new FileNotFoundException(
                    "Le fichier AllPrintings.json n'a pas été trouvé dans l'archive ZIP."
                );
            await using Stream jsonStream = entry.Open();
            using StreamReader sr = new(jsonStream);
            await using JsonTextReader jsonReader = new(sr);

            JsonSerializer serializer = new();

            JObject root = await JObject.LoadAsync(jsonReader);
            JToken? dataToken = root["data"];

            using HttpResponseMessage pricesStream = await httpClient.GetAsync(
                "https://mtgjson.com/api/v5/AllPricesToday.json"
            );

            await using Stream remoteStream = await pricesStream.Content.ReadAsStreamAsync();
            using MemoryStream pricesMemoryStream = new();
            await remoteStream.CopyToAsync(pricesMemoryStream);
            pricesMemoryStream.Position = 0;

            if (dataToken is JObject data)
            {
                await InsertSetsAsync(data);
                await InsertArtistsAsync(data);
                await InsertTypesAsync(data);
                await InsertSubTypesAsync(data);
                await InsertCardsAsync(data, pricesMemoryStream);
                await InsertCardTypeAssociationAsync(data);
                await InsertArtistCardAssociationAsync(data);
                await InsertCardSubTypeAssociationAsync(data);
                await InsertLanguagesAsync(data);
            }
        }
        catch (HttpRequestException)
        {
            throw new HttpRequestException(
                "Une erreur est survenue pendant la requête HTTP.",
                null,
                HttpStatusCode.InternalServerError
            );
        }
    }

    private async Task InsertSetsAsync(JObject data)
    {
        List<Set> sets = [];

        var existingSets = await _applicationDbContext.Sets.ToDictionaryAsync(s => s.SetCode);

        foreach (JProperty set in data.Properties())
        {
            JObject setObject = (JObject)set.Value;

            string setName = (string)setObject["name"]!;

            sets.Add(
                new Set
                {
                    Id = existingSets.TryGetValue(set.Name, out var existingSet)
                        ? existingSet.Id
                        : Guid.NewGuid(),
                    Name = setName,
                    SetCode = set.Name,
                }
            );
        }

        await _applicationDbContext.BulkMergeAsync(sets);
    }

    private async Task InsertArtistsAsync(JObject data)
    {
        HashSet<string> artistNames = [];

        var existingArtists = await _applicationDbContext.Artists.ToDictionaryAsync(a => a.Name);

        foreach (JProperty set in data.Properties())
        {
            foreach (JObject card in set.Value["cards"]!.Cast<JObject>())
            {
                string? artistField = (string)card["artist"]!;

                if (string.IsNullOrWhiteSpace(artistField))
                    continue;

                var names = artistField
                    .Split('&')
                    .Select(n => n.Trim())
                    .Where(n => !string.IsNullOrEmpty(n));

                foreach (string name in names)
                {
                    artistNames.Add(name);
                }
            }
        }

        List<Artist> artistsToInsert =
        [
            .. artistNames.Select(name => new Artist
            {
                Id = existingArtists.TryGetValue(name, out var existingArtist)
                    ? existingArtist.Id
                    : Guid.NewGuid(),
                Name = name,
            }),
        ];
        await _applicationDbContext.BulkMergeAsync(artistsToInsert);
    }

    private async Task InsertTypesAsync(JObject data)
    {
        HashSet<string> singleTypes = [];

        var existingTypes = await _applicationDbContext.Types.ToDictionaryAsync(a => a.Name);

        foreach (JProperty set in data.Properties())
        {
            foreach (JObject card in set.Value["cards"]!.Cast<JObject>())
            {
                string[]? types = [.. card["types"]!.Select(t => (string)t!)];

                if (types == null || types.Length == 0)
                    continue;

                foreach (var type in types)
                {
                    singleTypes.Add(type);
                }
            }
        }

        List<Type> typesToInsert =
        [
            .. singleTypes.Select(name => new Type
            {
                Id = existingTypes.TryGetValue(name, out var existingType)
                    ? existingType.Id
                    : Guid.NewGuid(),
                Name = name,
            }),
        ];
        await _applicationDbContext.BulkMergeAsync(typesToInsert);
    }

    private async Task InsertSubTypesAsync(JObject data)
    {
        HashSet<string> singleSubTypes = [];

        var existingSubTypes = await _applicationDbContext.SubTypes.ToDictionaryAsync(a => a.Name);

        foreach (JProperty set in data.Properties())
        {
            foreach (JObject card in set.Value["cards"]!.Cast<JObject>())
            {
                string[]? subTypes = [.. card["subtypes"]!.Select(t => (string)t!)];

                if (subTypes == null || subTypes.Length == 0)
                    continue;

                foreach (var subType in subTypes)
                {
                    singleSubTypes.Add(subType);
                }
            }
        }

        List<SubType> subTypesToInsert =
        [
            .. singleSubTypes.Select(name => new SubType
            {
                Id = existingSubTypes.TryGetValue(name, out var existingSubType)
                    ? existingSubType.Id
                    : Guid.NewGuid(),
                Name = name,
            }),
        ];
        await _applicationDbContext.BulkMergeAsync(subTypesToInsert);
    }

    private async Task InsertCardsAsync(JObject data, Stream cardesPrices)
    {
        List<Card> cards = [];

        //on utilise un dictionnaire pour la performance. On a pas à chercher dans la bd à chaque itération
        var sets = await _applicationDbContext.Sets.ToListAsync();
        var setDictionary = sets.ToDictionary(s => s.SetCode, s => s.Id);

        Dictionary<string, decimal> cardsPrice = GetPrices(cardesPrices);

        Dictionary<string, decimal> cardsFoilPrice = GetPrices(cardesPrices, true);

        foreach (JProperty set in data.Properties())
        {
            foreach (JObject card in set.Value["cards"]!.Cast<JObject>())
            {
                string? uuidStr = (string?)card["uuid"];

                if (string.IsNullOrEmpty(uuidStr))
                    continue;

                string scryfallId = (string)card["identifiers"]!["scryfallId"]!;

                string? text = card["foreignData"]
                    ?.FirstOrDefault(fd => (string?)fd["language"] == "French")
                    ?["text"]?.ToString();

                // Si pas trouvé en FR, on prend la carte principale
                if (string.IsNullOrEmpty(text))
                {
                    text = (string)card["text"]!;
                }

                string name = (string)card["name"]!;

                string? frenchName = card["foreignData"]
                    ?.FirstOrDefault(fd => (string?)fd["language"] == "French")
                    ?["name"]?.ToString();

                string setCode = (string)card["setCode"]!;
                Guid setId = setDictionary[setCode];

                decimal price = 0;
                cardsPrice.TryGetValue(uuidStr, out price);

                decimal foilPrice = 0;
                cardsFoilPrice.TryGetValue(uuidStr, out foilPrice);

                bool? onlineOnly =
                    card["availability"]?.Count() == 1
                    && (string?)card["availability"]?.First() == "mtgo";

                cards.Add(
                    new Card
                    {
                        Id = Guid.Parse(uuidStr),
                        SetId = setId,
                        ScryfallId = Guid.Parse(scryfallId!),
                        Name = name,
                        FrenchName = frenchName,
                        ManaCost = (string?)card["manaCost"],
                        Text = text,
                        Power = (string?)card["power"],
                        Toughness = (string?)card["toughness"],
                        Rarity = (string)card["rarity"]!,
                        Number = (string)card["number"]!,
                        HasFoil = (bool)card["hasFoil"]!,
                        HasNonFoil = (bool)card["hasNonFoil"]!,
                        Price = price,
                        FoilPrice = foilPrice,
                        OnlineOnly = onlineOnly ?? false,
                    }
                );

                if (cards.Count > 10000)
                {
                    await _applicationDbContext.BulkMergeAsync(cards);

                    cards.Clear();
                }
            }
        }
        if (cards.Count > 0)
            await _applicationDbContext.BulkMergeAsync(cards);
    }

    private async Task InsertCardTypeAssociationAsync(JObject data)
    {
        List<CardType> cardTypes = [];

        var types = await _applicationDbContext.Types.ToListAsync();
        var typeDictionary = types.ToDictionary(s => s.Name, s => s.Id);

        foreach (JProperty set in data.Properties())
        {
            foreach (JObject card in set.Value["cards"]!.Cast<JObject>())
            {
                string? uuidStr = (string?)card["uuid"];

                string[]? typeNames = [.. card["types"]!.Select(t => (string)t!)];

                if (string.IsNullOrEmpty(uuidStr) || typeNames == null || typeNames.Length == 0)
                    continue;

                foreach (string typeName in typeNames)
                {
                    Guid typeID = typeDictionary[typeName];

                    cardTypes.Add(new CardType { CardId = Guid.Parse(uuidStr), TypeId = typeID });
                }

                if (cardTypes.Count > 10000)
                {
                    await _applicationDbContext.BulkMergeAsync(cardTypes);

                    cardTypes.Clear();
                }
            }
        }

        if (cardTypes.Count > 0)
            await _applicationDbContext.BulkMergeAsync(cardTypes);
    }

    private async Task InsertArtistCardAssociationAsync(JObject data)
    {
        List<ArtistCard> artistCards = [];

        var artists = await _applicationDbContext.Artists.ToListAsync();
        var artistDictionary = artists.ToDictionary(s => s.Name, s => s.Id);

        foreach (JProperty set in data.Properties())
        {
            foreach (JObject card in set.Value["cards"]!.Cast<JObject>())
            {
                string? uuidStr = (string?)card["uuid"];

                string? artistField = (string)card["artist"]!;

                if (string.IsNullOrEmpty(uuidStr) || string.IsNullOrEmpty(artistField))
                    continue;

                var names = artistField
                    .Split('&')
                    .Select(n => n.Trim())
                    .Where(n => !string.IsNullOrEmpty(n));

                foreach (string name in names)
                {
                    Guid artistID = artistDictionary[name];

                    artistCards.Add(
                        new ArtistCard { CardId = Guid.Parse(uuidStr), ArtistId = artistID }
                    );
                }

                if (artistCards.Count > 10000)
                {
                    await _applicationDbContext.BulkMergeAsync(artistCards);

                    artistCards.Clear();
                }
            }
        }

        if (artistCards.Count > 0)
            await _applicationDbContext.BulkMergeAsync(artistCards);
    }

    private async Task InsertCardSubTypeAssociationAsync(JObject data)
    {
        List<CardSubType> cardSubTypes = [];

        var subTypes = await _applicationDbContext.SubTypes.ToListAsync();
        var subTypeDictionary = subTypes.ToDictionary(s => s.Name, s => s.Id);

        foreach (JProperty set in data.Properties())
        {
            foreach (JObject card in set.Value["cards"]!.Cast<JObject>())
            {
                string? uuidStr = (string?)card["uuid"];

                string[]? subTypeNames = [.. card["subtypes"]!.Select(t => (string)t!)];

                if (
                    string.IsNullOrEmpty(uuidStr)
                    || subTypeNames == null
                    || subTypeNames.Length == 0
                )
                    continue;

                foreach (string subTypeName in subTypeNames)
                {
                    Guid subTypeID = subTypeDictionary[subTypeName];

                    cardSubTypes.Add(
                        new CardSubType { CardId = Guid.Parse(uuidStr), SubTypeId = subTypeID }
                    );
                }

                if (cardSubTypes.Count > 10000)
                {
                    await _applicationDbContext.BulkMergeAsync(cardSubTypes);

                    cardSubTypes.Clear();
                }
            }
        }

        if (cardSubTypes.Count > 0)
            await _applicationDbContext.BulkMergeAsync(cardSubTypes);
    }

    private async Task InsertLanguagesAsync(JObject data)
    {
        List<Language> languages = [];

        foreach (JProperty set in data.Properties())
        {
            foreach (JObject card in set.Value["cards"]!.Cast<JObject>())
            {
                string? uuidStr = (string?)card["uuid"];
                JArray? foreignData = (JArray?)card["foreignData"];

                if (string.IsNullOrEmpty(uuidStr) || foreignData == null)
                    continue;

                foreach (JObject langData in foreignData.Cast<JObject>())
                {
                    string scryfallId = (string)langData["identifiers"]!["scryfallId"]!;
                    string name = (string)langData["language"]!;

                    languages.Add(
                        new Language
                        {
                            CardId = Guid.Parse(uuidStr),
                            ScryfallId = Guid.Parse(scryfallId),
                            Name = name,
                        }
                    );
                    if (languages.Count > 10000)
                    {
                        await MergeLanguagesAsync(languages);

                        languages.Clear();
                    }
                }
            }
        }
        if (languages.Count > 0)
            await MergeLanguagesAsync(languages);
    }

    private async Task MergeLanguagesAsync(List<Language> languages)
    {
        List<Guid> cardIds = [.. languages.Select(l => l.CardId).Distinct()];

        var existingLanguages = await _applicationDbContext
            .Languages.Where(l => cardIds.Contains(l.CardId))
            .ToListAsync();

        var existingLanguagesDictionary = existingLanguages.ToDictionary(
            l => new { l.CardId, l.Name },
            l => l
        );

        foreach (var lang in languages)
        {
            var key = new { lang.CardId, lang.Name };

            if (existingLanguagesDictionary.TryGetValue(key, out var existing))
                lang.Id = existing.Id;
            else
                lang.Id = Guid.NewGuid();
        }

        await _applicationDbContext.BulkMergeAsync(languages);
    }

    private static Dictionary<string, decimal> GetPrices(Stream downloadStream, bool foil = false)
    {
        var priceDictionary = new Dictionary<string, decimal>();

        if (downloadStream.CanSeek)
            downloadStream.Position = 0;

        using var sr = new StreamReader(downloadStream, leaveOpen: true);
        using var reader = new JsonTextReader(sr);

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value! == "data")
            {
                reader.Read();
                while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
                {
                    string cardId = (string)reader.Value!;
                    reader.Read();

                    JObject cardObj = JObject.Load(reader);

                    var prices = foil
                        ? cardObj["paper"]?["tcgplayer"]?["retail"]?["foil"]
                            ?? cardObj["paper"]?["cardkingdom"]?["retail"]?["foil"]
                        : cardObj["paper"]?["tcgplayer"]?["retail"]?["normal"]
                            ?? cardObj["paper"]?["cardkingdom"]?["retail"]?["normal"];

                    decimal mostRecentPrice = 0;

                    if (prices != null)
                    {
                        var lastProp = prices.Children<JProperty>().LastOrDefault();
                        if (lastProp != null)
                        {
                            mostRecentPrice = lastProp.Value.Value<decimal>();
                        }
                    }

                    priceDictionary[cardId] = mostRecentPrice;
                }
            }
        }

        return priceDictionary;
    }
}
