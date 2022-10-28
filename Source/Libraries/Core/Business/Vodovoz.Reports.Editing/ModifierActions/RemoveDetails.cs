using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class RemoveDetails : ModifierAction
	{
		private readonly string _tableName;

		public RemoveDetails(string tableName)
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
			var details = report.GetTable(_tableName, @namespace)
				.GetDetails(@namespace);

			details.Remove();
		}

		public override IEnumerable<ValidationResult> Validate(XDocument report)
		{
			yield break;
		}
	}
}
