using System;
using System.Collections.Generic;

namespace BohannanGEDCOMConsole
{
	interface IGedcomParser
	{
		void CreateCSV(List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList);
		List<CsvEntry> PopulateCSV(List<GeneGenie.Gedcom.GedcomIndividualRecord> familyList);
		List<GeneGenie.Gedcom.GedcomIndividualRecord> ParseGedcom(string[] args);
    }
}

