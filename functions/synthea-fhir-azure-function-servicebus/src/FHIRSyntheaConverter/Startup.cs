using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Conceptual.FHIRSyntheaConverter.Startup))]

namespace Conceptual.FHIRSyntheaConverter
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // TODO consider using singleton for "durable functions" pattern
            // builder.Services.AddSingleton<SyntheaMapper>();
        }
    }
}