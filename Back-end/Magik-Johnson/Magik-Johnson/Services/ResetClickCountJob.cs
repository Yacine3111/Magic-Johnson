using Magik_Johnson.Services.Interfaces;
using Quartz;

namespace Magik_Johnson.Services;

public class ResetClickCountJob : IResetClickCountJob
{
    private readonly ICardService _cardService;
    private readonly ILogger<ResetClickCountJob> _logger;

    public ResetClickCountJob(ICardService cardService, ILogger<ResetClickCountJob> logger)
    {
        _cardService = cardService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await ResetClickCountsAsync();
    }

    public async Task ResetClickCountsAsync()
    {
        try
        {
            _logger.LogInformation(
                "Début de la réinitialisation des compteurs de clics des cartes."
            );

            var rowsAffected = await _cardService.ResetAllClickCountsAsync();

            _logger.LogInformation(
                "Réinitialisation terminée avec succès. {Count} cartes ont été mises à jour.",
                rowsAffected
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Une erreur est survenue lors de la réinitialisation des compteurs de clics."
            );
            throw;
        }
    }
}
