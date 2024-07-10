using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class RenameElement : ModifierAction
	{
		private readonly string _elementOldName;
		private readonly string _elementNewName;

		public RenameElement(string elementOldName, string elementNewName)
		{
			if(string.IsNullOrWhiteSpace(elementOldName))
			{
				throw new ArgumentException($"'{nameof(elementOldName)}' cannot be null or whitespace.", nameof(elementOldName));
			}

			if(string.IsNullOrWhiteSpace(elementNewName))
			{
				throw new ArgumentException($"'{nameof(elementNewName)}' cannot be null or whitespace.", nameof(elementNewName));
			}

			_elementOldName = elementOldName;
			_elementNewName = elementNewName;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			report.RenameElement(_elementOldName, _elementNewName, @namespace);
		}
	}
}
