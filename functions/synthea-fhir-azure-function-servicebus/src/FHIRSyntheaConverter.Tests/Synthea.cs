using System;
using System.Collections.Generic;

namespace Conceptual.FHIRSyntheaConverter.Tests
{
    public class Patient
    {
        // Id,BIRTHDATE,DEATHDATE,SSN,DRIVERS,PASSPORT,PREFIX,FIRST,LAST,SUFFIX,MAIDEN,
        // MARITAL,RACE,ETHNICITY,GENDER,BIRTHPLACE,ADDRESS,CITY,STATE,COUNTY,ZIP,LAT,LON,HEALTHCARE_EXPENSES,HEALTHCARE_COVERAGE
        public string Id { get; set; }
        public string BIRTHDATE { get; set; }
        public string DEATHDATE { get; set; }
        public string SSN { get; set; }
        public string DRIVERS { get; set; }
        public string PASSPORT { get; set; }
        public string PREFIX { get; set; }
        public string FIRST { get; set; }
        public string LAST { get; set; }
        public string SUFFIX { get; set; }
        public string MAIDEN { get; set; }
        public string MARITAL { get; set; }
        public string RACE { get; set; }
        public string ETHNICITY { get; set; }
        public string GENDER { get; set; }
        public string BIRTHPLACE { get; set; }
        public string ADDRESS { get; set; }
        public string CITY { get; set; }
        public string STATE { get; set; }
        public string COUNTY { get; set; }
        public string ZIP { get; set; }
        public float LAT { get; set; }
        public float LON { get; set; }
        public float HEALTHCARE_EXPENSES { get; set; }
        public float HEALTHCARE_COVERAGE { get; set; }

        public List<Encounter> Encounters { get; set; }
        public List<Observation> Observations { get; set; }
        public List<Condition> Conditions { get; set; }
    }

    public class Encounter
    {
        // Id,START,STOP,PATIENT,ORGANIZATION,PROVIDER,PAYER,ENCOUNTERCLASS,CODE,DESCRIPTION,BASE_ENCOUNTER_COST,TOTAL_CLAIM_COST,PAYER_COVERAGE,REASONCODE,REASONDESCRIPTION
        public string Id { get; set; }
        // Just use string to avoid parsing -- we simply want this value passed to a JSON string
        public string START { get; set; }
        public string STOP { get; set; }
        public string PATIENT { get; set; }
        public string ORGANIZATION { get; set; }
        public string PROVIDER { get; set; }
        public string PAYER { get; set; }
        public string ENCOUNTERCLASS { get; set; }
        public string CODE { get; set; }
        public string DESCRIPTION { get; set; }
        public float BASE_ENCOUNTER_COST { get; set; }
        public float TOTAL_CLAIM_COST { get; set; }
        public float PAYER_COVERAGE { get; set; }
        public string REASONCODE { get; set; }
        public string REASONDESCRIPTION { get; set; }
    }

    public class Observation
    {
        // DATE,PATIENT,ENCOUNTER,CATEGORY,CODE,DESCRIPTION,VALUE,UNITS,TYPE
        public string DATE { get; set; }
        public string PATIENT { get; set; }
        public string ENCOUNTER { get; set; }
        public string CATEGORY { get; set; }
        public string CODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string VALUE { get; set; }
        public string UNITS { get; set; }
        public string TYPE { get; set; }
    }

    public class Condition
    {
        // START,STOP,PATIENT,ENCOUNTER,CODE,DESCRIPTION
        public string START { get; set; }
        public string STOP { get; set; }
        public string PATIENT { get; set; }
        public string ENCOUNTER { get; set; }
        public string CODE { get; set; }
        public string DESCRIPTION { get; set; }
    }
}
