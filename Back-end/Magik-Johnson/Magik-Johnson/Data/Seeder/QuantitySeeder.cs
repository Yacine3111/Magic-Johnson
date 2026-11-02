using Bogus;
using Magik_Johnson.Models;
using Microsoft.EntityFrameworkCore;

namespace Magik_Johnson.Data.Seeder;

public class QuantitySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Quantities.AnyAsync())
            return;

        var cards = await context.Cards.Include(c => c.Languages).ToListAsync();

        var languages = await context.Languages.ToListAsync();
        var faker = new Faker();

        var quantities = new List<Quantity>();

        foreach (var card in cards)
        {
            if (card.Languages.Count > 0)
            {
                foreach (var language in card.Languages)
                {
                    quantities.Add(
                        new Quantity
                        {
                            Id = Guid.NewGuid(),
                            LanguageId = language.Id,
                            CardId = card.Id,
                            NonFoilQuantity = faker.Random.Int(0, 100),
                            FoilQuantity = faker.Random.Bool(0.8f)
                                ? faker.Random.Int(0, 20)
                                : faker.Random.Int(21, 100),
                        }
                    );
                }
                quantities.Add(
                    new Quantity
                    {
                        Id = Guid.NewGuid(),
                        LanguageId = null,
                        CardId = card.Id,
                        NonFoilQuantity = faker.Random.Int(0, 100),
                        FoilQuantity = faker.Random.Bool(0.8f)
                            ? faker.Random.Int(0, 20)
                            : faker.Random.Int(21, 100),
                    }
                );
            }
            else
            {
                quantities.Add(
                    new Quantity
                    {
                        Id = Guid.NewGuid(),
                        LanguageId = null,
                        CardId = card.Id,
                        NonFoilQuantity = faker.Random.Int(0, 100),
                        FoilQuantity = faker.Random.Bool(0.8f)
                            ? faker.Random.Int(0, 20)
                            : faker.Random.Int(21, 100),
                    }
                );
            }
        }
        await context.Quantities.AddRangeAsync(quantities);
        await context.SaveChangesAsync();
    }
}
