using System;
using System.Collections.Generic;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Text;
using GeneGenie.Gedcom.Parser;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;

namespace BohannanGEDCOMConsole
{
	public class MultiThreadedGedcomParser : IGedcomParser
	{
        public MultiThreadedGedcomParser() { }

        public List<GeneGenie.Gedcom.GedcomIndividualRecord> ParseGedcom(string[] args)
        {
            Task[] tasks = new Task[args.Length - 2];

            var familyList = new List<GeneGenie.Gedcom.GedcomIndividualRecord>();
            var reader = GedcomRecordReader.CreateReader(args[0]);

            for (int i = 0; i < args.Length - 2; i++)
            {
                int innerI = i;
                tasks[i] = Task.Run(() =>
                {
                    List<GeneGenie.Gedcom.GedcomIndividualRecord> tempList = threadFunc(args, innerI, reader);
                    if (tempList.Count > 0) familyList.AddRange(tempList);
                });
            }

            Task.WaitAll(tasks);

            return familyList;
        }

        public void CreateCSV(List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList)
        {
            var culture = new CultureInfo("en-US");

            var dateSb = new StringBuilder(DateTime.Now.ToString(culture));
            dateSb.Replace("/", "_");
            dateSb.Replace(" ", "_");
            dateSb.Replace(":", "_");

            using (var writer = new StreamWriter($"GEDCOM_CSV_{dateSb}.csv"))
            using (var csv = new CsvWriter(writer, culture))
            {
                Console.WriteLine("Writing to CSV...");
                csv.Context.RegisterClassMap<CsvEntryMap>();
                csv.WriteRecords(GenerateCSVEntryList(familyList));
            }

            Console.WriteLine("Life finds a way.");
        }

        public List<GeneGenie.Gedcom.GedcomIndividualRecord> threadFunc(string[] args, int i, GedcomRecordReader reader)
        {
             var familyList = new List<GeneGenie.Gedcom.GedcomIndividualRecord>();

            List<GeneGenie.Gedcom.GedcomIndividualRecord> peeps = reader.Database.Individuals
                .Where(ind => ind.Names.Any(name => name.Surname == args[i + 1]))
                .ToList<GeneGenie.Gedcom.GedcomIndividualRecord>();

            if (peeps.Count > 0)
            {
                Console.WriteLine($"i: {i}");
                Console.WriteLine($"Adding {args[i + 1]} names...");
                familyList.AddRange(peeps);
            }

            return familyList;
        }

        public List<CsvEntry> GenerateCSVEntryList(List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList)
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
    }
}

