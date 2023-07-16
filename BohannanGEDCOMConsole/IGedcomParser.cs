using System;
using System.Collections.Generic;

namespace BohannanGEDCOMConsole
{
	public interface IGedcomParser
	{
		public List<CsvEntry> PopulateCSV(List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList);
        public void CreateCSV(List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList);
		public List<GeneGenie.Gedcom.GedcomIndividualRecord> ParseGedcom(string[] args);
    }
}

