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

            while (patientBundle != null)
            {
                Console.WriteLine($"Total: {patientBundle.Total} Entry count: {patientBundle.Entry.Count}");

                // list each patient in the bundle
                foreach (Bundle.EntryComponent entry in patientBundle.Entry)
                {
                    System.Console.WriteLine($" - Entry{patientNumber , 3}:{entry.FullUrl}");
                    //System.Console.WriteLine($"- {patient.Id,20} ");

                    if (entry.Resource != null)
                    {
                        Patient patient = (Patient)entry.Resource;
                        System.Console.WriteLine($" - Id: {patient.Id}");

                        if (patient.Name.Count > 0)
                        {
                           System.Console.WriteLine($" - Name: {patient.Name[0].ToString()}");
                        }
                    }

                    patientNumber ++;
                }

                // get more result
                patientBundle = fhirClient.Continue(patientBundle);
            }
            
        } 
    }
}