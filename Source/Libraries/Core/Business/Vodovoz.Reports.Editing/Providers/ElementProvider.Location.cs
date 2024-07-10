using System;
using System.Linq;
using System.Xml.Linq;

namespace Vodovoz.Reports.Editing.Providers
{
	public static partial class ElementProvider
	{
		private const string _leftPositionElementName = "Left";
		private const string _topPositionElementName = "Top";
		private const string _positionUnit = "pt";

		public static (decimal Left, decimal Top) GetElementPosition(this XContainer container, string elementName, string @namespace)
		{
			var element = CommonElementsExpressions.GetElementInContainerByName(container, elementName, @namespace);

			return GetElementPositionInPt(element, @namespace);
		}

		public static void SetElementPosition(this XContainer container, string elementName, string @namespace,
			decimal leftPositionInPt, decimal topPositionInPt)
		{
			var element = CommonElementsExpressions.GetElementInContainerByName(container, elementName, @namespace);

			SetElementLeftPositionValue(element, @namespace, leftPositionInPt);
			SetElementTopPositionValue(element, @namespace, topPositionInPt);
		}

		public static void MoveElementDown(this XContainer container, string elementName, string @namespace, decimal offsetInPt)
		{
			var element = CommonElementsExpressions.GetElementInContainerByName(container, elementName, @namespace);

			var topPositionValue = GetTopPositionValue(element, @namespace);

			SetElementTopPositionValue(element, @namespace, topPositionValue + offsetInPt);
		}

		private static (decimal Left, decimal Top) GetElementPositionInPt(XElement element, string @namespace)
		{
			var leftValue = GetLeftPositionValue(element, @namespace);
			var topValue = GetTopPositionValue(element, @namespace);

			return (leftValue, topValue);
		}

		private static decimal GetLeftPositionValue(XElement element, string @namespace)
		{
			var positionElement = GetLeftPositionElement(element, @namespace);
			var positionValue = GetPositionInPtElementValue(positionElement);

			return positionValue;
		}

		private static decimal GetTopPositionValue(XElement element, string @namespace)
		{
			var positionElement = GetTopPositionElement(element, @namespace);
			var positionValue = GetPositionInPtElementValue(positionElement);

			return positionValue;
		}

		private static decimal GetPositionInPtElementValue(XElement element)
		{
			if(element is null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if(element.Name.LocalName != _leftPositionElementName
				&& element.Name.LocalName != _topPositionElementName)
			{
				throw new InvalidOperationException("Аргумент не является элементом положения");
			}

			var postitonValue = element.Value;

			if(!postitonValue.EndsWith(_positionUnit))
			{
				var errorMessage = $"Единица измерения значения положения должна быть {_positionUnit}";
				throw new InvalidOperationException(errorMessage);
			}

			if(!decimal.TryParse(postitonValue.Substring(0, postitonValue.Length - 2), out var value))
			{
				throw new InvalidOperationException("Ошибка при парсинге числа в элементе позиции");
			}

			return value;
		}

		private static void SetElementLeftPositionValue(XElement element, string @namespace, decimal valueInPt)
		{
			var leftPositionElement = GetLeftPositionElement(element, @namespace);
			leftPositionElement.Value = $"{valueInPt}{_positionUnit}";
		}

		private static void SetElementTopPositionValue(XElement element, string @namespace, decimal valueInPt)
		{
			var topPositionElement = GetTopPositionElement(element, @namespace);
			topPositionElement.Value = $"{valueInPt}{_positionUnit}";
		}

		private static XElement GetLeftPositionElement(XElement element, string @namespace)
		{
			return CommonElementsExpressions.GetChildElement(element, @namespace, _leftPositionElementName);
		}

		private static XElement GetTopPositionElement(XElement element, string @namespace)
		{
			return CommonElementsExpressions.GetChildElement(element, @namespace, _topPositionElementName);
		}
	}
}
