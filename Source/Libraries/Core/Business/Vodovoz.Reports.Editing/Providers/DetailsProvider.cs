using System;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.RDL.Elements;
using Vodovoz.RDL.Utilities;

namespace Vodovoz.Reports.Editing.Providers
{
	public class DetailsProvider
	{
		public TableRow GetDetailsRow(XDocument report, string tableName)
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

		public TableRow GetSecondHeaderRow(XDocument report, string tableName)
		{
			var ns = report.Root.Attribute("xmlns").Value;
			var rows = report
				.GetTable(tableName, ns)
				.GetHeader(ns)
				.GetTableRows(ns);

			if(!rows.Any())
			{
				throw new InvalidOperationException($"В таблице {tableName} в разделе Header отсутствуют строки");
			}

			if(rows.Elements().Count() < 2)
			{
				throw new InvalidOperationException($"В таблице {tableName} в разделе Header должно быть 2 строки");
			}

			var row = rows.Elements().Skip(1).First();
			var result = row.FromXElement<TableRow>();
			return result;
		}
	}
}
