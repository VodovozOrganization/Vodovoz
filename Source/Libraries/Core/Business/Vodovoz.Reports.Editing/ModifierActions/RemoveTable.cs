using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class RemoveTable : ModifierAction
	{
		private readonly string _tableName;

		public RemoveTable(string tableName)
		{
			if(string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"'{nameof(tableName)}' cannot be null or whitespace.", nameof(tableName));
			}

			_tableName = tableName;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			var table = report.GetTable(_tableName, @namespace);
			
			if (table != null)
			{
				table.Remove();
			}
		}
	}
}
