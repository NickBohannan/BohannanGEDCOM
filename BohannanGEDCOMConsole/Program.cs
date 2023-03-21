using System;
using System.Linq;
using GeneGenie.Gedcom.Parser;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;

namespace BohannanGEDCOMConsole
{
    public class FamilyNames
    {
        public IEnumerable<GeneGenie.Gedcom.GedcomIndividualRecord> BohananNames;
        public IEnumerable<GeneGenie.Gedcom.GedcomIndividualRecord> BohannonNames;
        public IEnumerable<GeneGenie.Gedcom.GedcomIndividualRecord> BohannanNames;
    }

    public class CsvEntry
    {
        public string FirstName;
        public string LastName;
        public string Address;
        public string Birthday;
        public string IsAlive; // yes or no to keep it simple for Uncle Rich
    }

    public class CsvEntryMap : ClassMap<CsvEntry>
    {
        public CsvEntryMap()
        {
            Map(m => m.FirstName).Index(0).Name("FirstName");
            Map(m => m.LastName).Index(1).Name("LastName");
            Map(m => m.Address).Index(2).Name("Address");
            Map(m => m.Birthday).Index(3).Name("Birthday");
            Map(m => m.IsAlive).Index(4).Name("IsAlive");
        }
    }

    class Program
    {
        static FamilyNames GetFamilyNames(GedcomRecordReader reader)
        {
            try
            {
                FamilyNames family = new FamilyNames
                {
                    BohananNames = reader.Database.Individuals.Where(ind => ind.Names.Any(name => name.Surname == "Bohanan")),
                    BohannonNames = reader.Database.Individuals.Where(ind => ind.Names.Any(name => name.Surname == "Bohannon")),
                    BohannanNames = reader.Database.Individuals.Where(ind => ind.Names.Any(name => name.Surname == "Bohannan"))
                };

                return family.BohananNames.Count() > 0 ? family : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        static List<CsvEntry> PopulateCSV(IEnumerable<GeneGenie.Gedcom.GedcomIndividualRecord> nameList)
        {
            var tempList = new List<CsvEntry>();

            foreach (GeneGenie.Gedcom.GedcomIndividualRecord record in nameList)
            {
                tempList.Add(new CsvEntry()
                {
                    FirstName = record.Names[0]?.Given,
                    LastName = record.Names[0]?.Surname,
                    Address = $"{record.Address?.AddressLine1} {record.Address?.AddressLine2} {record.Address?.AddressLine3} {record.Address?.City}, {record.Address?.State} {record.Address?.PostCode}",
                    Birthday = record.Birth?.Date?.DateString,
                    IsAlive = (record.Dead) ? "No" : "Yes"
                });
            }
            return tempList;
        }

        static void Main(string[] args)
        {
            var gedcomReader = GedcomRecordReader.CreateReader("/Users/nickbohannan/Downloads/Bohannan_03_03_2023.ged");

            if (gedcomReader.Parser.ErrorState != GeneGenie.Gedcom.Enums.GedcomErrorState.NoError)
            {
                Console.WriteLine($"Could not read file, encountered error {gedcomReader.Parser.ErrorState}.");
            }

            var familyNames = GetFamilyNames(gedcomReader); // Bohanan Bohannan Bohannon

            if (familyNames != null)
            {
                Console.WriteLine($"Bohanan: {familyNames.BohananNames}");
                Console.WriteLine($"Bohannon: {familyNames.BohannonNames}");
                Console.WriteLine($"Bohannan: {familyNames.BohannanNames}\n");
            }
            else
            {
                Console.WriteLine($"No names found in Gedcom or exception occurred. Please check console.");
            }

            using (var writer = new StreamWriter("BoNames.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<CsvEntryMap>();
                csv.WriteRecords(PopulateCSV(familyNames.BohannanNames));
                csv.WriteRecords(PopulateCSV(familyNames.BohannonNames));
                csv.WriteRecords(PopulateCSV(familyNames.BohananNames));
            }
        }

        // Given - surname - address - 
    }
}
