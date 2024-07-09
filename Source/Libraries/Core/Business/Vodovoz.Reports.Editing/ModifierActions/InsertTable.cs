using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class InsertTable : ModifierAction
	{
		private readonly string _table;

		public InsertTable(string table)
		{
			if(string.IsNullOrWhiteSpace(table))
			{
				throw new ArgumentException($"'{nameof(table)}' cannot be null or whitespace.", nameof(table));
			}

			_table = table;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			var table = report.GetTable("TableSales", @namespace);

			report.InsertTable(table, @namespace);
		}
	}
}
