using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class RenameTable : ModifierAction
	{
		private readonly string _tableOldName;
		private readonly string _tableNewName;

		public RenameTable(string tableOldName, string tableNewName)
		{
			if(string.IsNullOrWhiteSpace(tableOldName))
			{
				throw new ArgumentException($"'{nameof(tableOldName)}' cannot be null or whitespace.", nameof(tableOldName));
			}

			if(string.IsNullOrWhiteSpace(tableNewName))
			{
				throw new ArgumentException($"'{nameof(tableNewName)}' cannot be null or whitespace.", nameof(tableNewName));
			}

			_tableOldName = tableOldName;
			_tableNewName = tableNewName;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			report.RenameTable(_tableOldName, _tableNewName, @namespace);
		}
	}
}
