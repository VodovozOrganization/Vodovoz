using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class RemoveElement : ModifierAction
	{
		private readonly ElementType _elementType;
		private readonly string _elementName;

		public RemoveElement(ElementType elementType, string elementName)
		{
			if(string.IsNullOrWhiteSpace(elementName))
			{
				throw new ArgumentException($"'{nameof(elementName)}' cannot be null or whitespace.", nameof(elementName));
			}
			_elementType = elementType;
			_elementName = elementName;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;

			var element = report.GetElementByTypeAndNameAttribute(_elementType, _elementName, @namespace);

			element?.Remove();
		}
	}
}

