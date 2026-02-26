using System.Collections.Generic;
using System.Threading.Tasks;
using TrueStage.Core.Models;

namespace TrueStage.Core.Interfaces;

public interface ITargetWriter
{
    Task WriteRowsAsync(IEnumerable<MappedRow> rows, IngestionContext context);
    Task WriteErrorAsync(MappedRow row, IngestionContext context, string errorMessage);
    Task CreateIngestionJobAsync(IngestionContext context);
    Task CloseIngestionJobAsync(IngestionContext context, string status);
}
