using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

using Microsoft.Azure.ServiceBus;

namespace Conceptual.FHIRSyntheaConverter.Tests
{
    public class Tests
    {
        private const string LIQUID_TEMPLATE_NAME = "FHIRToSynthea";
        private static readonly string CURRENT_DIRECTORY = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        private static readonly string SYNTHEA_DATA_DIRECTORY = Path.Join(CURRENT_DIRECTORY, "/data/synthea").ToString();
        private static readonly string INPUT_DATA_DIRECTORY = Path.Join(CURRENT_DIRECTORY, "/data/sample input").ToString();
        private static readonly string EXPECTED_OUTPUT_DIRECTORY = Path.Join(CURRENT_DIRECTORY, "/data/expected output").ToString();
        private static readonly string ACTUAL_OUTPUT_DIRECTORY = Path.Join(CURRENT_DIRECTORY, "/data/actual output").ToString();
        private readonly FHIRtoSyntheaQueueTrigger _trigger;
        private readonly ILogger _logger = new TestLogger<FHIRtoSyntheaQueueTrigger>();

        public Tests()
        {
            SyntheaCSVtoJSON.ConvertSyntheaCSVstoJSON(INPUT_DATA_DIRECTORY, SYNTHEA_DATA_DIRECTORY, EXPECTED_OUTPUT_DIRECTORY);
            _trigger = new FHIRtoSyntheaQueueTrigger();
        }

        public static IEnumerable<object[]> GetFHIRData()
        {
            var sampleFileNames = Directory.GetFiles(INPUT_DATA_DIRECTORY, "*.json").Select(f => Path.GetFileName(f));
            var expectedFileNames = Directory.GetFiles(EXPECTED_OUTPUT_DIRECTORY, "*.json").Select(f => Path.GetFileName(f));
            foreach (var fileName in sampleFileNames)
            {
                var sampleFile = Path.Join(INPUT_DATA_DIRECTORY, fileName);
                var expectedFile = Path.Join(EXPECTED_OUTPUT_DIRECTORY, "Synthea_" + fileName);
                // Different templates can be specified here for testing
                yield return new [] { LIQUID_TEMPLATE_NAME, sampleFile, expectedFile};
            }
        }

        // Transformation only
        [Theory]
        [MemberData(nameof(GetFHIRData))]
        public void GivenFHIRData_WhenConverting_ExpectedSyntheaResourceShouldBeReturned(string rootTemplate, string inputFile, string expectedFile)
        {
            var inputContent = File.ReadAllText(inputFile);
            var convertedSyntheaContent = SyntheaMapper.ConvertFHIRToSynthea(inputContent, rootTemplate);
            System.Diagnostics.Debug.WriteLine(convertedSyntheaContent);

            var expectedContent = File.ReadAllText(expectedFile);

            // Write syntheaContent JSON to file for review
            var fileName = Path.GetFileName(inputFile);
            var newOutputFilePath = Path.Join(ACTUAL_OUTPUT_DIRECTORY, "Output_" + fileName);
            File.WriteAllText(newOutputFilePath, convertedSyntheaContent);
            
            // Check that these are the same
            var actualObject = JObject.Parse(convertedSyntheaContent);
            var expectedObject = JObject.Parse(expectedContent);
            Assert.True(JToken.DeepEquals(expectedObject, actualObject));
        }

        // Function local trigger
        [Theory (Skip = "TODO")]
        [MemberData(nameof(GetFHIRData))]
        public void GivenFHIRData_WhenTriggeringFunction_ThenExpectedSyntheaResourceShouldBeSent(string rootTemplate, string inputFile, string expectedFile)
        {
            var inputContent = File.ReadAllText(inputFile);
            // var message = new Message(System.Text.Encoding.UTF8.GetBytes(inputContent));
            _trigger.Run(inputContent, _logger);
            // TODO Check that message exists at destination
        }

        // Send to Service Bus in Azure
        [Theory (Skip = "TODO")]
        [MemberData(nameof(GetFHIRData))]
        public void GivenFHIRData_WhenReceivingMessage_ThenExpectedSyntheaResourceShouldBeSent(string rootTemplate, string inputFile, string expectedFile)
        {
            var inputContent = File.ReadAllText(inputFile);
            var message = new Message(System.Text.Encoding.UTF8.GetBytes(inputContent));
            // Get the connection string from app settings
            var connectionString = Environment.GetEnvironmentVariable("AzureServiceBusConnectionString");
            var queueName = Environment.GetEnvironmentVariable("AzureServiceBusQueueName");
            var queueClient = new QueueClient(connectionString, queueName);
            queueClient.SendAsync(message);
            // TODO Check that message exists at destination
        }
    }

}

