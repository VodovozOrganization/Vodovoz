using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class RemoveFooter: ModifierAction
	{
		private readonly string _tableName;

		public RemoveFooter(string tableName)
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
			var footer = report.GetTable(_tableName, @namespace)
				.GetFooter(@namespace);

			footer.Remove();
		}
	}
}
