using System;
using System.Collections.Generic;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Text;
using GeneGenie.Gedcom.Parser;
using System.Linq;

namespace BohannanGEDCOMConsole
{
	public class SingleThreadedGedcomParser : IGedcomParser
	{
		public SingleThreadedGedcomParser()
		{
		}

        public List<CsvEntry> PopulateCSV(List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList)
        {
            var tempList = new List<CsvEntry>();

            foreach (GeneGenie.Gedcom.GedcomIndividualRecord record in familyList)
            {
                if (record != null)
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
            }

            return tempList;
        }

        public void CreateCSV(List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList)
        {
            var culture = new CultureInfo("en-US");

            StringBuilder dateSb = new StringBuilder(DateTime.Now.ToString(culture));
            dateSb.Replace("/", "_");
            dateSb.Replace(" ", "_");
            dateSb.Replace(":", "_");

            using (var writer = new StreamWriter($"GEDCOM_CSV_{dateSb}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                Console.WriteLine("Writing to CSV...");
                csv.Context.RegisterClassMap<CsvEntryMap>();
                csv.WriteRecords(PopulateCSV(familyList));
            }
        }

        public List<GeneGenie.Gedcom.GedcomIndividualRecord> ParseGedcom(string[] args)
        {
            var reader = GedcomRecordReader.CreateReader(args[0]);

            List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList = new();

            for (int i = 0; i < args.Length - 1; i++)
            {
                familyList.AddRange(reader.Database.Individuals.Where(ind => ind.Names.Any(name => name.Surname == args[i + 1])));
            }

            return familyList;
        }
    }
}

