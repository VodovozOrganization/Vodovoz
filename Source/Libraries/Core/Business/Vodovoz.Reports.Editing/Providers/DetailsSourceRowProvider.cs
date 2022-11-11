using System;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.RDL.Elements;
using Vodovoz.RDL.Utilities;

namespace Vodovoz.Reports.Editing.Providers
{
	public class DetailsSourceRowProvider : SourceRowProvider
	{
		public override TableRow GetSourceRow(XDocument report, string tableName)
		{
			var ns = report.Root.Attribute("xmlns").Value;
			var rows = report
				.GetTable(tableName, ns)
				.GetDetails(ns)
				.GetTableRows(ns);

			if(!rows.Any())
			{
				throw new InvalidOperationException($"В таблице {tableName} в разделе Details отсутствуют строки");
			}

			var row = rows.Elements().First();
			var result = row.FromXElement<TableRow>();
			return result;
		}
	}
}
