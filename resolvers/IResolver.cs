using Microsoft.Azure.DigitalTwins.Parser;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    public interface IResolver
    {
        Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis);
    }
}