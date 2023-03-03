using System;
using System.Linq;
using GeneGenie.Gedcom.Parser;

namespace BohannanGEDCOMConsole
{
    class FamilyNamesCount
    {
        public int BohananNames;
        public int BohannonNames;
        public int BohannanNames;
        public int TotalNames;
    }

    class Program
    {
        static FamilyNamesCount CountFamilyNames(GedcomRecordReader reader)
        {
            try
            {
                FamilyNamesCount family = new FamilyNamesCount
                {
                    BohananNames = reader.Database.Individuals.Where(ind => ind.Names.Any(name => name.Surname == "Bohanan")).Count(),
                    BohannonNames = reader.Database.Individuals.Where(ind => ind.Names.Any(name => name.Surname == "Bohannon")).Count(),
                    BohannanNames = reader.Database.Individuals.Where(ind => ind.Names.Any(name => name.Surname == "Bohannan")).Count(),
                    TotalNames = reader.Database.Individuals.Count()
                };

                return family.TotalNames > 0 ? family : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        static void Main(string[] args)
        {
            var gedcomReader = GedcomRecordReader.CreateReader("/Users/nickbohannan/Downloads/Bohannan_03_03_2023.ged");

            if (gedcomReader.Parser.ErrorState != GeneGenie.Gedcom.Enums.GedcomErrorState.NoError)
            {
                Console.WriteLine($"Could not read file, encountered error {gedcomReader.Parser.ErrorState}.");
            }

            var familyNamesCount = CountFamilyNames(gedcomReader); // Bohanan Bohannan Bohannon

            if (familyNamesCount != null)
            {
                Console.WriteLine($"Bohanan: {familyNamesCount.BohananNames}");
                Console.WriteLine($"Bohannon: {familyNamesCount.BohannonNames}");
                Console.WriteLine($"Bohannan: {familyNamesCount.BohannanNames}\n");
                Console.WriteLine($"Total: {familyNamesCount.TotalNames}");
            }
            else
            {
                Console.WriteLine($"No names found in Gedcom or exception occurred. Please check console.");
            }
        }
    }
}
