using System;
using System.Linq;
using System.Xml.Linq;

namespace Vodovoz.Reports.Editing.Providers
{
	public static partial class ElementProvider
	{
		public static (decimal Left, decimal Top) GetElementPosition(this XContainer container, string elementName, string @namespace)
		{
			var element = GetElementInContainerByName(container, elementName, @namespace);

			return GetElementPosition(element, @namespace);
		}

		public static bool SetElementPosition(this XContainer container, string elementName, string @namespace,
			decimal leftPosition, decimal topPosition)
		{
			var element = GetElementInContainerByName(container, elementName, @namespace);

			return true;
		}

		private static (decimal Left, decimal Top) GetElementPosition(XElement element, string @namespace)
		{
			var leftElement = LeftPositionElement(element, @namespace);
			var topElement = TopPositionElement(element, @namespace);

			if(!decimal.TryParse(leftElement?.Value, out var leftValue))
			{
				throw new InvalidOperationException("Ошибка при парсинге числа в элементе Left");
			}

			if(!decimal.TryParse(null, out var topValue))
			{
				throw new InvalidOperationException("Ошибка при парсинге числа в элементе Top");
			}

			return (leftValue, topValue);
		}

		private static XElement GetElementInContainerByName(XContainer container, string elementName, string @namespace)
		{
			var elements = container.Descendants(XName.Get(elementName, @namespace));

			if(!elements.Any())
			{
				throw new InvalidOperationException("Элемент с указанным имененм не найден!");
			}

			if(elements.Count() > 1)
			{
				throw new InvalidOperationException("Найдено несколько элементов с указанным имененем!");
			}

			var element = elements.First();

			return element;
		}

		private static XElement LeftPositionElement(XElement element, string @namespace) =>
			element.Descendants(XName.Get("Left", @namespace)).Where(e => e.Parent == element).FirstOrDefault();

		private static XElement TopPositionElement(XElement element, string @namespace) =>
			element.Descendants(XName.Get("Top", @namespace)).Where(e => e.Parent == element).FirstOrDefault();
	}
}
