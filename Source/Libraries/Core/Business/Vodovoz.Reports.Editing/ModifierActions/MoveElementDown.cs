using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class MoveElementDown : ModifierAction
	{
		private readonly string _elementName;
		private readonly ElementType _elementType;
		private readonly double _offsetInPt;

		public MoveElementDown(string elementName, ElementType elementType, double offsetInPt)
		{
			if(string.IsNullOrWhiteSpace(elementName))
			{
				throw new ArgumentException($"'{nameof(elementName)}' cannot be null or whitespace.", nameof(elementName));
			}
			_elementName = elementName;
			_elementType = elementType;
			_offsetInPt = offsetInPt;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			report.MoveElementDown(_elementType, _elementName, @namespace, _offsetInPt);
		}
	}
}

