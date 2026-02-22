using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using System.Collections.Generic;



namespace fhir_cs_tutorial_01
{
    /// <summary>
    /// Main Entry point for the programm 
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

        static int Main(string[] args)
        {
            FhirClient fhirClient = new FhirClient(_fhirServer)
            {
                PreferredFormat = ResourceFormat.Json,
                PreferredReturn = Prefer.ReturnRepresentation
            };

            CreatPatient(fhirClient,"Mansour" , "Emami");
            List<Patient> patients = GetPatients(fhirClient);

            System.Console.WriteLine($"Found {patients.Count}");

            string firstId = null;

            foreach (Patient patient in patients)
            {
                if (string.IsNullOrEmpty(firstId))
                {
                    firstId = patient.Id;
                    continue;
                }

                DeletePatient(fhirClient, patient.Id);
            }

            Patient firstPatient = ReadPatient(fhirClient, firstId);
            System.Console.WriteLine($"Read back patient : {firstPatient.Name[0].ToString()}");
            
            UpdatePatient(fhirClient, firstPatient);

            Patient updated = UpdatePatient(fhirClient, firstPatient);
            Patient readFinal = ReadPatient(fhirClient, firstId);

            return 0;
        }

        /// <summary>
        /// Update  a given patient 
        /// </summary>
        /// <param name="fhirClient"></param>
        /// <param name="patient"></param>
        /// <returns>an Updated Patient</returns>
        static Patient UpdatePatient(FhirClient fhirClient, Patient patient)
        {
            patient.Telecom.Add(new ContactPoint()
            {
                System = ContactPoint.ContactPointSystem.Phone,
                Value = "01748445659",
                Use = ContactPoint.ContactPointUse.Home,
            });

            patient.Gender = AdministrativeGender.Male;

            return fhirClient.Update<Patient>(patient);
        }

        /// <summary>
        /// Read a patient from a fhir server , by id 
        /// </summary>
        /// <param name="fhirClient"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        static Patient ReadPatient(FhirClient fhirClient, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return fhirClient.Read<Patient>($"Patient/{id}");
        }
        
        /// <summary>
        /// Delete Patient specified by id
        /// </summary>
        /// <param name="fhirClient"></param>
        /// <param name="id"></param>
        static void DeletePatient(FhirClient fhirClient , string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            fhirClient.Delete($"Patient/{id}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fhirClient"></param>
        /// <param name="FamilyName"></param>
        /// <param name="givenName"></param>
        static void CreatPatient(FhirClient fhirClient , string FamilyName , string givenName)
        {
            Patient toCreate = new Patient()
            {
                Name = new List<HumanName>()
                {
                    new HumanName()
                    {
                        Family = FamilyName,
                        Given = new List<string>()
                        {
                            givenName,
                        },
                    }
                },
                BirthDateElement = new Date(1970, 01,01),
            };

            Patient created = fhirClient.Create<Patient>(toCreate);
            System.Console.WriteLine($"Created Patient/{created.Id}");
        }

        /// <summary>
        ///     Get List of patient matching the specified criteria
        /// </summary>
        /// <param name="fhirClient"></param>
        /// <param name="patientCriteria"></param>
        /// <param name="maxPatient">maximu number of patient to return ( default is 20) </param>
        /// <param name="onlyWithencounter">Flag to only return patients with Encounters (def false)</param>
        /// <returns></returns>
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