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

        private static readonly Dictionary<string , string> _fhirServers = new Dictionary<string , string>()
        {
            {"PublicVonk", "http://vonk.fire.ly"},
            {"PublicHAPI","https://hapi.fhir.org/baseR4"},
            {"Local","http://localhost:8080/fhir"}
        };

        private static readonly string _fhirServer = _fhirServers["Local"];

        static void Main(string[] args)
        {
            FhirClient fhirClient = new FhirClient(_fhirServer)
            {
                PreferredFormat = ResourceFormat.Json,
                PreferredReturn = Prefer.ReturnRepresentation
            };

            List<Patient> patients = GetPatients(fhirClient);

            System.Console.WriteLine($"Found {patients.Count}");
        }

        static List<Patient> GetPatients(
            FhirClient fhirClient,
            string[] patientCriteria = null,
            int maxPatient = 20,
            bool onlyWithencounter=false

        )
        {
            List<Patient> patients = new List<Patient>();

            Bundle patientBundle;
            if ((patientCriteria == null )|| (patientCriteria.Length == 0))
            {
                patientBundle = fhirClient.Search<Patient>();

            }
            else
            {
                patientBundle  = fhirClient.Search<Patient>(patientCriteria);

            }


            
            List<string> patientsWithEncounters = new List<string>();

            while (patientBundle != null)
            {
                Console.WriteLine($"patient Bundle.Total: {patientBundle.Total} Entry count: {patientBundle.Entry.Count}");

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

                        if (onlyWithencounter && encounterBundle.Total == 0)
                        {
                            continue;
                        }

                        patients.Add(patient);

                        System.Console.WriteLine($" - Entry{patients.Count, 3}:{entry.FullUrl}");
                        System.Console.WriteLine($"- {patient.Id} ");

                        if (patient.Name.Count > 0)
                        {
                           System.Console.WriteLine($" - Name: {patient.Name[0].ToString()}");
                        }

                        if (encounterBundle.Total > 0)
                        {
                        Console.WriteLine($" - Encounter Total: {encounterBundle.Total} Entry count: {patientBundle.Entry.Count}");
                        }
                    }

                    

                    if (patients.Count >= maxPatient)
                    {
                        break;
                    }

                }

                if (patients.Count >= maxPatient)
                {
                    break;
                }

                // get more result
                patientBundle = fhirClient.Continue(patientBundle);
            }
            
            return patients;
        } 
    }
}