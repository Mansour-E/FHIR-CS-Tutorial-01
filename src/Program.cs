using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;

namespace fhir_cs_tutorial_01
{
    public static class Program
    {
        private const string _fhirServer = "http://vonk.fire.ly";
        static void Main(string[] args)
        {
            FhirClient fhirClient = new FhirClient(_fhirServer);
            
        }
    }
}

