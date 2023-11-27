using System;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class SetTextboxPosition : ModifierAction
	{
		private readonly string _textboxName;
		private readonly double _leftValueInPt;
		private readonly double _topValueInPt;

		public SetTextboxPosition(string textboxName, double leftValueInPt, double topValueInPt)
		{
			if(string.IsNullOrWhiteSpace(textboxName))
			{
				throw new ArgumentException($"'{nameof(textboxName)}' cannot be null or whitespace.", nameof(textboxName));
			}

			_textboxName = textboxName;
			_leftValueInPt = leftValueInPt;
			_topValueInPt = topValueInPt;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			var textBox = report.GetTextbox(_textboxName, @namespace);

			var leftContainer = textBox.Descendants(XName.Get("Left", @namespace)).FirstOrDefault();
			var topContainer = textBox.Descendants(XName.Get("Top", @namespace)).FirstOrDefault();

			if(leftContainer != null)
			{
				leftContainer.Value = $"{_leftValueInPt}pt";
			}

			if(topContainer != null)
			{
				topContainer.Value = $"{_topValueInPt}pt";
			}
		}
	}
}
