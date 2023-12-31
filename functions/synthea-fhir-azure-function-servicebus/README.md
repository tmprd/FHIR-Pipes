# FHIR to Synthea using Azure Function & Service Bus Queue
# Development
### Requirements
* .NET Core 6.0+
* [FHIR-Converter](https://github.com/microsoft/FHIR-Converter.git)
    * This relies heavily on [Dotliquid](https://github.com/dotliquid/dotliquid), which could be used instead
* Optional: [Azure CLI](https://github.com/Azure/azure-cli) or Azure Powershell
* Optional: [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Build & Deploy
```shell
# No nuget package available, so clone this directly
git clone https://github.com/microsoft/FHIR-Converter.git deps/FHIR-Converter
dotnet restore deps/FHIR-Converter
dotnet build deps/FHIR-Converter

# Now run our solution
dotnet restore
dotnet clean
dotnet build
dotnet test src/FHIRSyntheaConverter.Tests

# Deploy with ARM template
sh deploy.sh

# Optionally run locally, but see https://github.com/Azure/azure-functions-core-tools/issues/2232 
cd src/FHIRSyntheaConverter
func start
```

### TODO
- [ ] Complete mappings require the full SyntheticMass dataset (20GB+)
- [ ] Finish configurations for Azure Function, Service Bus, and ARM Template

# Architecture
### Mapping Implementation
* Declarative mapping using [Liquid Templates](https://shopify.github.io/liquid/)
    * Provides operations like mapping and filtering for querying, traversal, and transformation
    * Comparable to [FHIRPath](https://hl7.org/fhir/fhirpath.html)
* Selected fake data from [Synthea](https://synthea.mitre.org/) is converted from FHIR R4 JSON Bundles into JSON versions of Synthea's data model
    * FHIR is more complex than Synthea, so many FHIR elements do not have a corresponding mapping in Synthea

### Event-Driven ETL
* Messages containing Synthea FHIR JSON are sent to a Service Bus Queue
    * These could instead contain references to updated FHIR resources which could be queried
* The Service Bus triggers an Azure Function to process the message
* The Azure Function transforms the FHIR JSON into Synthea's (original? native?) data model, encoded in JSON
* The transformation, using Liquid Templates, could instead be done using a custom $convert-data operation in a FHIR server

### Infrastructure as Code
* ARM (Azure Resource Manager) Template


# Review
* Advantages
    * Declarative mappings are easier to add, update, read, and reason about
    * Liquid Templates available for HL7v2, CDA
* Disadvantages
    * Liquid Templates are not intuitive, difficult to debug
    * Likely slower than imperative mapping
    * Inefficient composition:
        * No combined filtering with mapping, e.g. `where: "resource.resourceType ... `
            * Instead requires `map: "resource" | where: "resourceType` which is less efficient
        * No nested filters, e.g. something like `items.where(items => items.encounters.any(encounter => encounter ...))`
            * Instead requires nested `for` loops
        * Can't efficiently select from the first value of filtered collection, e.g. `where: "system", "https://github.com/synthetichealth/synthea" | first | value `
            * Instead requires `where: "system", "https://github.com/synthetichealth/synthea" | map: "value" | first }}"`
        * Can't set empty strings as default values
            * To work around this, the Synthea JSON we generate and the expected JSON we compare against both leave empty values out. If we wanted to then convert the generated JSON back into CSV, it might be easier to instead generate empty strings for null values. But Liquid has no good way of doing this.


# Synthea Mapping Coverage
- [ ] allergies
- [ ] careplans
- [ ] claims_transactions
- [ ] claims
- [x] conditions
- [ ] devices
- [x] encounters
    * Except associated provider and payer
- [ ] imaging_studies
- [ ] immunizations
- [ ] medications
- [x] observations
- [ ] organizations
- [x] patients
    * Except expense and coverage sums
    * Extension code mappings are incomplete
- [ ] payer_transitions
- [ ] payers
- [ ] procedures
- [ ] providers
- [ ] supplies
