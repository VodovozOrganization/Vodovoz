using System;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class CopyElement : ModifierAction
	{
		private readonly ElementType _elementType;
		private readonly string _sourceElementName;
		private readonly string _newElementName;

		public CopyElement(ElementType elementType, string sourceElementName, string newElementName)
		{
			if(string.IsNullOrWhiteSpace(sourceElementName))
			{
				throw new ArgumentException($"'{nameof(sourceElementName)}' cannot be null or whitespace.", nameof(sourceElementName));
			}

			if(string.IsNullOrWhiteSpace(newElementName))
			{
				throw new ArgumentException($"'{nameof(newElementName)}' cannot be null or whitespace.", nameof(newElementName));
			}
			_elementType = elementType;
			_sourceElementName = sourceElementName;
			_newElementName = newElementName;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;

			var sourceElement = report.GetElementByTypeAndNameAttribute(_elementType, _sourceElementName, @namespace);

			var newElement = new XElement(sourceElement);
			newElement.Attribute("Name").Value = _newElementName;

			report.InsertElementIntoReportItems(newElement, @namespace);
		}
	}
}

