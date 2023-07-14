using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection.PortableExecutable;
using CsvHelper;
using CsvHelper.Configuration;
using GeneGenie.Gedcom.Parser;

namespace BohannanGEDCOMConsole
{
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
        static List<GeneGenie.Gedcom.GedcomIndividualRecord> GetAllFamilyNames(GedcomRecordReader reader, string[] args)
        {
            List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList = new List<GeneGenie.Gedcom.GedcomIndividualRecord>();

            try
            {
                for (int i = 1; i < args.Length; i++)
                {
                    Console.WriteLine($"Adding {args[i]} names...");
                    int prevLength = familyList.Count();
                    familyList.AddRange(reader.Database.Individuals.Where(ind => ind.Names.Any(name => name.Surname == args[i])));
                    Console.WriteLine($"Added {familyList.Count() - prevLength}");
                }

                return familyList.Count() > 0 ? familyList : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        static List<CsvEntry> PopulateCSV(List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList)
        {
            var tempList = new List<CsvEntry>();

            foreach (GeneGenie.Gedcom.GedcomIndividualRecord record in familyList)
            {
                tempList.Add(new CsvEntry()
                {
                    FirstName = record.Names[0]?.Given,
                    LastName = record.Names[0]?.Surname,
                    Address = $"{record.Address?.City}, {record.Address?.State}",
                    Birthday = record.Birth?.Date?.DateString,
                    IsAlive = (record.Dead) ? "No" : "Yes"
                });
            }
            return tempList;
        }

        static void Main(string[] args)
        {
            var gedcomReader = GedcomRecordReader.CreateReader(args[0]);

            if (gedcomReader.Parser.ErrorState != GeneGenie.Gedcom.Enums.GedcomErrorState.NoError)
            {
                Console.WriteLine($"Could not read file, encountered error {gedcomReader.Parser.ErrorState}.");
            }

            List<GeneGenie.Gedcom.GedcomIndividualRecord> familyNames = GetAllFamilyNames(gedcomReader, args);

            var culture = new CultureInfo("en-US");

            StringBuilder dateSb = new StringBuilder(DateTime.Now.ToString(culture));
            dateSb.Replace("/", "_");
            dateSb.Replace(" ", "_");
            dateSb.Replace(":", "_");

            using (var writer = new StreamWriter($"GEDCOM_CSV_{dateSb}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<CsvEntryMap>();
                csv.WriteRecords(PopulateCSV(familyNames));
            }
        }
    }
}
