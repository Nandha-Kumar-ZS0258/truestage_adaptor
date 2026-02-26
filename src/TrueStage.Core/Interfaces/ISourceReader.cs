using System.Collections.Generic;
using System.IO;
using TrueStage.Core.Models;

namespace TrueStage.Core.Interfaces;

public interface ISourceReader
{
    IEnumerable<Dictionary<string, string>> ReadRows(Stream stream, CuMappingConfig config);
}
