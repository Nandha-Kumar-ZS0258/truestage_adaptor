using System.Threading.Tasks;
using TrueStage.Core.Models;

namespace TrueStage.Core.Interfaces;

public interface IConfigLoader
{
    Task<CuMappingConfig> LoadConfigAsync(string cuId);
}
