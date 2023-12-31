using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Conceptual.FHIRSyntheaConverter.Tests
{
    public static class SyntheaCSVtoJSON
    {
        /// <summary>
        /// For each FHIR JSON file in the directory, find its corresponding Synthea CSV files, then create Synthea JSON files from them.
        /// </summary>
        public static void ConvertSyntheaCSVstoJSON(string intputDirectoryPath, string csvDirectoryPath, string outputDirectoryPath)
        {            
            // Get all FHIR JSON files
            var fhirFiles = Directory.GetFiles(intputDirectoryPath, "*.json");
            // Get all patient IDs from these
            var patientIds = fhirFiles.Aggregate(new List<string>(), (acc, fhirFile) => {
                // (Assuming Synthea JSON files end with patient ID)
                // Ex. Jerrold404_Herzog843_217f95a3-4e10-bd5d-fb67-0cfb5e8ba075.json
                var filenameSplit = Path.GetFileName(fhirFile).Split('_');
                // Skip files not matching this pattern
                if (filenameSplit.Length < 3) return acc;
                var patientId = filenameSplit[2].Split('.')[0];
                acc.Add(patientId);
                return acc;
            }).ToList();

            if (patientIds.Count == 0)
            {
                Console.WriteLine("No FHIR JSON files found.");
                return;
            }

            // Continue only if some FHIR JSON file is missing its corresponding expected Synthea JSON file
            var expectedSyntheaFiles = Directory.GetFiles(outputDirectoryPath, "*.json");
            var expectedSyntheaPatientIds = expectedSyntheaFiles.Aggregate(new List<string>(), (acc, syntheaFile) => {
                var filenameSplit = Path.GetFileName(syntheaFile).Split('_');
                if (filenameSplit.Length < 4) return acc;
                var patientId = filenameSplit[3].Split('.')[0];
                acc.Add(patientId);
                return acc;
            }).ToList();
            var missingPatientIds = patientIds.Except(expectedSyntheaPatientIds).ToList();
            if (missingPatientIds.Count == 0)
            {
                return;
            }

            Console.WriteLine("Generating Synthea JSON from CSV files...");

            // Read CSV files
            List<Patient> patients = ReadCsv<Patient>(Path.Join(csvDirectoryPath, "patients.csv"));
            List<Encounter> encounters = ReadCsv<Encounter>(Path.Join(csvDirectoryPath, "encounters.csv"));
            List<Observation> observations = ReadCsv<Observation>(Path.Join(csvDirectoryPath, "observations.csv"));
            List<Condition> conditions = ReadCsv<Condition>(Path.Join(csvDirectoryPath, "conditions.csv"));

            // Select patients with IDs in the list
            var targetPatients = patients.Where(p => patientIds.Contains(p.Id.ToString())).ToList();
            if (targetPatients.Count == 0)
            {
                Console.WriteLine("No patients found.");
                return;
            }
            
            // Find relata
            foreach (var patient in targetPatients)
            {
                patient.Encounters = encounters.Where(e => e.PATIENT == patient.Id).ToList();
                patient.Observations = observations.Where(o => o.PATIENT == patient.Id).ToList();
                patient.Conditions = conditions.Where(o => o.PATIENT == patient.Id).ToList();

                // Don't serialize null values (as empty strings), just leave them out. Otherwise, the Liquid Templates will have to also insert empty strings as default values.
                var settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                };
                string json = JsonConvert.SerializeObject(patient, Formatting.Indented, settings);
                var outputFilename = $"Synthea_{patient.FIRST}_{patient.LAST}_{patient.Id}.json";
                var outputJsonPath = Path.Join(outputDirectoryPath, outputFilename);
                File.WriteAllText(outputJsonPath, json);
                Console.WriteLine($"Generated {outputFilename}.");
            }
        }
        
        // CSV & JSON tools
        private static List<T> ReadCsv<T>(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Convert empty values to null
                csvReader.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues.Add(string.Empty);
                return csvReader.GetRecords<T>().ToList();
            }
        }
    }
}