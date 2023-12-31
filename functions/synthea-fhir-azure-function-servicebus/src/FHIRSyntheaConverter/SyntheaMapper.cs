using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Conceptual.FHIRSyntheaConverter
{
    public static class SyntheaMapper
    {
        private const string LIQUID_TEMPLATE_NAME = "FHIRToSynthea";
        private const string TEMPLATE_DIRECTORY = "./mappings/";
        private static readonly ProcessorSettings _processorSettings = new ProcessorSettings();
        private static readonly JsonProcessor jsonProcessor = new JsonProcessor(_processorSettings);
        private static readonly TemplateProvider templateProvider = new TemplateProvider(TEMPLATE_DIRECTORY, DataType.Json);

        public static string ConvertFHIRToSynthea(string fhirContent, string templateName = LIQUID_TEMPLATE_NAME)
        {
            // Could pick template based on FHIR resource type, but we're only using patient bundles now

            // Transform FHIR into Synthea using Liquid template
            return jsonProcessor.Convert(fhirContent, templateName, templateProvider);
        }
    }
}