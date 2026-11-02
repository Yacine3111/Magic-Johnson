namespace Magik_Johnson.Extensions;

public static class LanguageHelper
{
    private static readonly Dictionary<string, string> LanguageMap = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        { "Anglais", "English" },
        { "Français", "French" },
        { "Allemand", "German" },
        { "Espagnol", "Spanish" },
        { "Italien", "Italian" },
        { "Portugais", "Portuguese" },
        { "Portugais (Brésil)", "Portuguese (Brazil)" },
        { "Russe", "Russian" },
        { "Japonais", "Japanese" },
        { "Coréen", "Korean" },
        { "Chinois simplifié", "Chinese Simplified" },
        { "Chinois traditionnel", "Chinese Traditional" },
        { "Grec ancien", "Ancient Greek" },
        { "Arabe", "Arabic" },
        { "Hébreu", "Hebrew" },
        { "Latin", "Latin" },
        { "Sanskrit", "Sanskrit" },
        { "Phyrexian", "Phyrexian" },
    };

    public static string ToEnglish(string frenchLanguage)
    {
        if (string.IsNullOrWhiteSpace(frenchLanguage))
            return frenchLanguage;

        return LanguageMap.TryGetValue(frenchLanguage, out var english) ? english : frenchLanguage;
    }
}
