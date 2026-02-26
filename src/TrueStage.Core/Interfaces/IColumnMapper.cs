using System.Collections.Generic;
using TrueStage.Core.Models;

namespace TrueStage.Core.Interfaces;

public interface IColumnMapper
{
    MappedRow MapRow(Dictionary<string, string> sourceRow, CuMappingConfig config, int rowNumber);
}
