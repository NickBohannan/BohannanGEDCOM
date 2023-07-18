/************************************************************
 *                                                          *
 *      BOHANNAN MULTITHREADED GEDCOM SURNAME PARSER        *
 *                                                          *
 ***********************************************************/

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
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Concurrent;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Diagnostics;

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
        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();

            // SingleThreadedGedcomParser gParser = new SingleThreadedGedcomParser();
            MultiThreadedGedcomParser gParser = new MultiThreadedGedcomParser();

            List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList = gParser.ParseGedcom(args);
            gParser.CreateCSV(familyList);
            sw.Stop();
            Console.WriteLine($"Elapsed Millipoobles: {sw.ElapsedMilliseconds.ToString()}");

            Console.ReadKey();
        }
    }
}

// CLOJURE ISSUE

//Its the closure you've got there that closes over your for loop variable.

//That i variable is promoted at compile time .. because its a loop counter and its actually accessed outside of the loop (in the thread delegate here):

//Thread t = new Thread(() =>
//{
//    UdpPortListener(Convert.ToUInt16(52000 + i));
//}); //                                      ^^^ the compiler closes over this
//What this means, is that by the time your Threads are spawned up the value of i is checked in your UdpPortListener method...the value of i is the last value in the for loop..because the loop executed before it.

//To fix this .. you need to copy the value inside loop:

//var temp = i;
//Thread t = new Thread(() =>
//{
//    UdpPortListener(Convert.ToUInt16(52000 + temp));
//});
//
// CREDIT: Simon Whitehead
