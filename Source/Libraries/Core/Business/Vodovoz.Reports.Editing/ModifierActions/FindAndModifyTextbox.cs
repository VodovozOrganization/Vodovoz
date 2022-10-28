using System;
using System.Xml.Linq;
using Vodovoz.RDL.Elements;
using Vodovoz.RDL.Utilities;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class FindAndModifyTextbox : ModifierAction
	{
		private readonly string _textBoxName;
		private readonly Action<Textbox> _action;

		public FindAndModifyTextbox(string textBoxName, Action<Textbox> action)
		{
			if(string.IsNullOrWhiteSpace(textBoxName))
			{
				throw new ArgumentException($"'{nameof(textBoxName)}' cannot be null or whitespace.", nameof(textBoxName));
			}

			_textBoxName = textBoxName;
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			var textBoxElement = report.GetTextbox(_textBoxName, @namespace);
			var textBox = textBoxElement.FromXElement<Textbox>();
			_action(textBox);
			var modifiedTextboxElement = textBox.ToXElement<Textbox>();
			textBoxElement.ReplaceWith(modifiedTextboxElement);
		}
	}
}
