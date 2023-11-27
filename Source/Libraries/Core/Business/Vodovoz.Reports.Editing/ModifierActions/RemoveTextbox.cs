using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class RemoveTextbox : ModifierAction
	{
		private readonly string _textboxName;

		public RemoveTextbox(string textboxName)
		{
			if(string.IsNullOrWhiteSpace(textboxName))
			{
				throw new ArgumentException($"'{nameof(textboxName)}' cannot be null or whitespace.", nameof(textboxName));
			}

			_textboxName = textboxName;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			var textBox = report.GetTextbox(_textboxName, @namespace);

			textBox?.Remove();
		}
	}
}
