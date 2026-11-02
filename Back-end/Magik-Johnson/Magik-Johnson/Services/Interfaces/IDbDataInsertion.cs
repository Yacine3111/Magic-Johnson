using Quartz;

namespace Magik_Johnson.Services.Interfaces;

public interface IDbDataInsertion : IJob
{
    Task InsertDataAsync();
}
