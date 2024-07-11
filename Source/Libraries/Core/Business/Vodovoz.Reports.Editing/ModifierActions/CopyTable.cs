using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class CopyTable : ModifierAction
	{
		private readonly string _sourceTableName;
		private readonly string _newTableName;

		public CopyTable(string sourceTableName, string newTableName)
		{
			if(string.IsNullOrWhiteSpace(sourceTableName))
			{
				throw new ArgumentException($"'{nameof(sourceTableName)}' cannot be null or whitespace.", nameof(sourceTableName));
			}

			if(string.IsNullOrWhiteSpace(newTableName))
			{
				throw new ArgumentException($"'{nameof(newTableName)}' cannot be null or whitespace.", nameof(newTableName));
			}

			_sourceTableName = sourceTableName;
			_newTableName = newTableName;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			var sourceTable = report.GetTable(_sourceTableName, @namespace);

			var newTable = new XElement(sourceTable);
			newTable.Attribute("Name").Value = _newTableName;

			report.InsertTable(newTable, @namespace);
		}
	}
}
