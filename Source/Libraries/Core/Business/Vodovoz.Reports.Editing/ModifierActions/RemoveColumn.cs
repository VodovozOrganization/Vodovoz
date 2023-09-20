using System;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class RemoveColumn : ModifierAction
	{
		private readonly string _tableName;
		private readonly string _cellMarkerName;

		public RemoveColumn(string tableName, string cellMarkerName)
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
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			var table = report.GetTable(_tableName, @namespace);
			var columns = table.GetTableColumns(@namespace);

			var indexToErase = table.GetTextBoxColumnIndex(_cellMarkerName, @namespace);
			
			if(indexToErase == -1)
			{
				throw new InvalidOperationException($"TextBox {_cellMarkerName} not found");
			}

			columns.Elements().ElementAt(indexToErase).Remove();

			var rowsSets = table.GetTableRows(@namespace);

			foreach(var rowSet in rowsSets)
			{
				foreach(var row in rowSet.Elements())
				{
					var cells = row.GetRowCells(@namespace).Elements();

					cells.ElementAt(indexToErase).Remove();
				}
			}
		}
	}
}
