using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Vodovoz.RDL.Elements;

namespace Vodovoz.RDL.Providers
{
	public class TableColumnProvider
	{
		private readonly XDocument _report;
		private readonly string _namespace;

		public TableColumnProvider(XDocument report)
		{
			_report = report ?? throw new ArgumentNullException(nameof(report));
			_namespace = _report.Root.Attribute("xmlns").Value;
		}

		public int GetTotalTableColumns(string tableName)
		{
			var tables = _report.Descendants(XName.Get("Table", _namespace));
			if(!tables.Any())
			{
				throw new InvalidOperationException("В отчете отсутствуют таблицы");
			}

			var matchedTables = tables.Where(x => x.Attribute(XName.Get("Name")).Value == tableName);
			if(!matchedTables.Any())
			{
				throw new InvalidOperationException($"В отчете отсутствуют таблицы с именем {tableName}");
			}

			if(matchedTables.Count() > 1)
			{
				throw new InvalidOperationException($"В отчете присутствуют несколько таблиц с именем {tableName}");
			}
			var table = matchedTables.First();
			var tableColumns = table.Descendants(XName.Get("TableColumn", _namespace));

			return tableColumns.Count();
		}
	}
}
