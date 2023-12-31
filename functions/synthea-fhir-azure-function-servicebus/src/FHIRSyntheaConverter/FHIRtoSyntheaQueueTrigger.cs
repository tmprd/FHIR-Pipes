using System;
using System.Text;
using Microsoft.Extensions.Logging;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.ServiceBus;

namespace Conceptual.FHIRSyntheaConverter
{
    public class FHIRtoSyntheaQueueTrigger
    {
        [FunctionName("FHIRtoSyntheaQueueTrigger")]
        [return: ServiceBus("syntheaJSONQueue", Connection = "OutgoingServiceBusConnectionString")]
        public string Run([ServiceBusTrigger("syntheaFHIRQueue", Connection = "IncomingServiceBusConnectionString")] string message, ILogger log)
        {
            log.LogInformation("Received: {message}", message);

            // TODO validate
            var fhirContent = message;
            
            // FHIR -> Synthea
            var syntheaContent = SyntheaMapper.ConvertFHIRToSynthea(fhirContent);

            System.Diagnostics.Debug.WriteLine(syntheaContent);

            // Send to a different service bus queue using output binding specified above
            return syntheaContent;
        }
    }
}
