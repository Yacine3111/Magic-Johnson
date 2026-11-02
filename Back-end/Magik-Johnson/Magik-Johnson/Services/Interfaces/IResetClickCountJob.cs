using Quartz;

namespace Magik_Johnson.Services.Interfaces;

public interface IResetClickCountJob : IJob
{
    Task ResetClickCountsAsync();
}
