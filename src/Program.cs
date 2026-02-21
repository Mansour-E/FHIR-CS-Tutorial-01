using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using System.Collections.Generic;



namespace fhir_cs_tutorial_01
{
    /// <summary>
    /// Main program
    /// </summary>
    public static class Program
    {
        private const string _fhirServer = "https://hapi.fhir.org/baseR4"; // https://hapi.fhir.org/baseR4".

        static void Main(string[] args)
        {
            FhirClient fhirClient = new FhirClient(_fhirServer)
            {
                PreferredFormat = ResourceFormat.Json,
                PreferredReturn = Prefer.ReturnRepresentation
            };
            

            Bundle patientBundle = fhirClient.Search<Patient>(new string[] {"name=test"});

            int patientNumber = 0;
            List<string> patientsWithEncounters = new List<string>();

            while (patientBundle != null)
            {
                Console.WriteLine($"Total: {patientBundle.Total} Entry count: {patientBundle.Entry.Count}");

                // list each patient in the bundle
                foreach (Bundle.EntryComponent entry in patientBundle.Entry)
                {

                    //
                    //

                    if (entry.Resource != null)
                    {
                        Patient patient = (Patient)entry.Resource;
                        //System.Console.WriteLine($" - Id: {patient.Id}");

                        Bundle encounterBundle = fhirClient.Search<Encounter>(new string[]{$"patient=Patient/{patient.Id}", });

                        if (encounterBundle.Total == 0)
                        {
                            continue;
                        }

                        patientsWithEncounters.Add(patient.Id);

                        System.Console.WriteLine($" - Entry{patientNumber, 3}:{entry.FullUrl}");
                        System.Console.WriteLine($"- {patient.Id} ");

                        if (patient.Name.Count > 0)
                        {
                           System.Console.WriteLine($" - Name: {patient.Name[0].ToString()}");
                        }

                        Console.WriteLine($" - Encounter Total: {encounterBundle.Total} Entry count: {patientBundle.Entry.Count}");

                    }

                    patientNumber ++;

                    if (patientsWithEncounters.Count >= 3)
                    {
                        break;
                    }

                }

                if (patientsWithEncounters.Count >= 3)
                {
                    break;
                }

                // get more result
                patientBundle = fhirClient.Continue(patientBundle);
            }
            
        } 
    }
}