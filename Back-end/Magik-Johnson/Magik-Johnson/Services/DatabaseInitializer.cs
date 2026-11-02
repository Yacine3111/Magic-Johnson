using Magik_Johnson.Data;
using Magik_Johnson.Models;
using Newtonsoft.Json;

namespace Magik_Johnson.Services;

/*
    classe qui permet de "seeder" les pays et provinces pour la page "Adresse"
 */
public static class DatabaseInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Countries.Any())
        {
            var chemin = Path.Combine(AppContext.BaseDirectory, "countries.json");

            string json = File.ReadAllText(chemin);

            var countries = JsonConvert.DeserializeObject<List<Country>>(json);
            if (countries != null)
            {
                context.Countries.AddRange(countries);
                context.SaveChanges();
            }
        }
    }
}
