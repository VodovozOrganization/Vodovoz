using System;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class SetColumnWidth : ModifierAction
	{
		private readonly string _tableName;
		private readonly string _cellMarkerName;
		private readonly int _columnWidthValueInPt;

		public SetColumnWidth(string tableName, string cellMarkerName, int columnWidthValueInPt)
		{
			if(string.IsNullOrWhiteSpace(cellMarkerName))
			{
				throw new ArgumentException($"{nameof(cellMarkerName)} cannot be empty.", nameof(cellMarkerName));
			}

			if(string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"'{nameof(tableName)}' cannot be null or whitespace.", nameof(tableName));
			}

			_tableName = tableName;
			_cellMarkerName = cellMarkerName;
			_columnWidthValueInPt = columnWidthValueInPt;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			var table = report.GetTable(_tableName, @namespace);
			var columns = table.GetTableColumns(@namespace);

			var columnToResizeIndex = table.GetTextBoxColumnIndex(_cellMarkerName, @namespace);

			if(columnToResizeIndex == -1)
			{
				throw new InvalidOperationException($"TextBox {_cellMarkerName} not found");
			}

			var c = columns.Elements().ElementAt(columnToResizeIndex);
			var widthContainer = c.Descendants(XName.Get("Width", @namespace)).FirstOrDefault();

			if(widthContainer != null)
			{
				widthContainer.Value = $"{_columnWidthValueInPt}pt";
			}
		}
	}
}
