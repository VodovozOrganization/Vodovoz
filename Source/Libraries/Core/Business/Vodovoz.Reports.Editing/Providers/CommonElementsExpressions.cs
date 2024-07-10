using System;
using System.Linq;
using System.Xml.Linq;

namespace Vodovoz.Reports.Editing.Providers
{
	public static class CommonElementsExpressions
	{
		public static XElement GetElementInContainerByName(XContainer container, string elementName, string @namespace)
		{
			var elements = container.Descendants(XName.Get(elementName));

			if(!elements.Any())
			{
				throw new InvalidOperationException("Элемент с указанным имененм не найден");
			}

			if(elements.Count() > 1)
			{
				throw new InvalidOperationException("Найдено несколько элементов с указанным имененем");
			}

			var element = elements.First();

			return element;
		}

		public static XElement GetChildElement(XElement element, string @namespace, string childElementName)
		{
			if(element is null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			var childElement =
				element.Descendants(XName.Get(childElementName))
				.Where(e => e.Parent == element)
				.FirstOrDefault();

			if(childElement is null)
			{
				var errorMessage = $"Элемент \"{childElementName}\" не найден";
				throw new InvalidOperationException(errorMessage);
			}

			return childElement;
		}
	}
}
