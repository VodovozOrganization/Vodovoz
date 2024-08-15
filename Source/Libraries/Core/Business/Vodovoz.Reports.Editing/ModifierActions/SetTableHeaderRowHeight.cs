using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class SetTableHeaderRowHeight : ModifierAction
	{
		private readonly string _tableName;
		private readonly double _tableHeaderFirstRowHeightInPt;

		public SetTableHeaderRowHeight(string tableName, double tableHeaderFirstRowHeightInPt)
		{
			if(string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"'{nameof(tableName)}' cannot be null or whitespace.", nameof(tableName));
			}

			_tableName = tableName;
			_tableHeaderFirstRowHeightInPt = tableHeaderFirstRowHeightInPt;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;

			var tableHeaderRows = report.GetTableHeaderRows(_tableName, @namespace);
			var firstHeaderRow = tableHeaderRows.First();

			var headerRowHeightElement = firstHeaderRow.GetSingleChildElement("Height", @namespace);
			headerRowHeightElement.Value = $"{_tableHeaderFirstRowHeightInPt.ToString("N2", CultureInfo.InvariantCulture)}pt";
		}
	}
}

