using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class SetElementPosition : ModifierAction
	{
		private readonly string _elementName;
		private readonly decimal _leftPositionInPt;
		private readonly decimal _topPositionInPt;

		public SetElementPosition(string elementName, decimal leftPositionInPt, decimal topPositionInPt)
		{
			if(string.IsNullOrWhiteSpace(elementName))
			{
				throw new ArgumentException($"'{nameof(elementName)}' cannot be null or whitespace.", nameof(elementName));
			}
			_elementName = elementName;
			_leftPositionInPt = leftPositionInPt;
			_topPositionInPt = topPositionInPt;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			report.SetElementPosition(_elementName, @namespace, _leftPositionInPt, _topPositionInPt);
		}
	}
}


