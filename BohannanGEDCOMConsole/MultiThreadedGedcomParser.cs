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
		public MultiThreadedGedcomParser()
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

        public List<GeneGenie.Gedcom.GedcomIndividualRecord> threadFunc(string[] args, int i, GedcomRecordReader reader)
        {
            List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList = new();

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

            Console.WriteLine("Life finds a way.");
        }

        public List<GeneGenie.Gedcom.GedcomIndividualRecord> ParseGedcom(string[] args)
        {
            Console.WriteLine("Entering mt parsegedcom method...");
            Task[] tasks = new Task[args.Length - 2];

            List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList = new();
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
            // Add to ConcurrentBag concurrently
            //ConcurrentBag<List<GeneGenie.Gedcom.GedcomIndividualRecord>> listBag = new ConcurrentBag<List<GeneGenie.Gedcom.GedcomIndividualRecord>>();
            //ConcurrentBag<GedcomRecordReader> readerBag = new ConcurrentBag<GedcomRecordReader>();

            //List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList = new();

            //List<Task> bagAddTasks = new List<Task>();

            //for (int i = 0; i < args.Length - 1; i++)
            //{
            //    int innerI = i;
            //    bagAddTasks.Add(Task.Run(() => listBag.Add(new List<GeneGenie.Gedcom.GedcomIndividualRecord>())));
            //    bagAddTasks.Add(Task.Run(() => readerBag.Add(GedcomRecordReader.CreateReader($"{args[0].Substring(0, 46)}_{innerI}.ged"))));
            //}

            //// Wait for all tasks to complete
            //Task.WaitAll(bagAddTasks.ToArray());



            //Console.WriteLine("Finished adding tasks...");

            //// Consume the items in the bag
            //List<Task> bagConsumeTasks = new List<Task>();

            //for (int i = 0; i < args.Length - 1; i++)
            //{
            //    int itemsInBag = i;
            //    bagConsumeTasks.Add(Task.Run(() =>
            //    {
            //        List<GeneGenie.Gedcom.GedcomIndividualRecord> listItem;
            //        GedcomRecordReader readerItem;

            //        if (listBag.TryTake(out listItem) && readerBag.TryTake(out readerItem))
            //        {
            //            listItem = threadFunc(args, itemsInBag, readerItem);
            //            familyList.AddRange(listItem);
            //        }
            //    }));
            //}

            //Task.WaitAll(bagConsumeTasks.ToArray());

            return familyList;
        }
    }
}

