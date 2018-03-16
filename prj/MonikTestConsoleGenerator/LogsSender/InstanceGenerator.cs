using Monik.Client;
using MonikService.Core.Repository;

namespace MonikTestConsoleGenerator.LogsSender
{
    public class InstanceGenerator
    {
        public Instance                   Instance      { get; set; }
        public MonikTestGeneratorInstance ClientControl { get; set; }
    }
}