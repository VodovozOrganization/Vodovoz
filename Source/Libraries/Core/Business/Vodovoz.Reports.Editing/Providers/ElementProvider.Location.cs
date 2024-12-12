using System;
using System.Globalization;
using System.Xml.Linq;

namespace Vodovoz.Reports.Editing.Providers
{
	public static partial class ElementProvider
	{
		private const string _leftPositionElementName = "Left";
		private const string _topPositionElementName = "Top";
		private const string _positionUnit = "pt";

		public static (double Left, double Top) GetTablePosition(this XContainer container, string tableName, string @namespace)
		{
			var table = container.GetTable(tableName, @namespace);

			return GetElementPositionInPt(table, @namespace);
		}

		public static void SetTablePosition(this XContainer container, string tableName, string @namespace,
			double leftPositionInPt, double topPositionInPt)
		{
			var element = container.GetTable(tableName, @namespace);

			SetElementLeftPositionValue(element, leftPositionInPt, @namespace);
			SetElementTopPositionValue(element, topPositionInPt, @namespace);
		}

		public static void MoveElementVertically(this XContainer container, ElementType elementType,
			string elementName, string @namespace, double offsetInPt)
		{
			var element = container.GetElementByTypeAndNameAttribute(elementType, elementName, @namespace);

			var topPositionValue = GetTopPositionValue(element, @namespace);

			SetElementTopPositionValue(element, topPositionValue + offsetInPt, @namespace);
		}

		private static (double Left, double Top) GetElementPositionInPt(XElement element, string @namespace)
		{
			var leftValue = GetLeftPositionValue(element, @namespace);
			var topValue = GetTopPositionValue(element, @namespace);

			return (leftValue, topValue);
		}

		private static double GetLeftPositionValue(XElement element, string @namespace)
		{
			var positionElement = GetLeftPositionElement(element, @namespace);
			var positionValue = GetPositionInPtElementValue(positionElement);

			return positionValue;
		}

		private static double GetTopPositionValue(XElement element, string @namespace)
		{
			var positionElement = GetTopPositionElement(element, @namespace);
			var positionValue = GetPositionInPtElementValue(positionElement);

			return positionValue;
		}

		private static double GetPositionInPtElementValue(XElement element)
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

			var numberFormat = new NumberFormatInfo
			{
				NumberDecimalSeparator = "."
			};

			if(!double.TryParse(postitonValue.Substring(0, postitonValue.Length - 2), NumberStyles.Any, numberFormat, out var value))
			{
				throw new InvalidOperationException("Ошибка при парсинге числа в элементе позиции");
			}

			return value;
		}

		private static void SetElementLeftPositionValue(XElement element, double valueInPt, string @namespace)
		{
			var leftPositionElement = GetLeftPositionElement(element, @namespace);
			leftPositionElement.Value = $"{valueInPt.ToString("0.00", CultureInfo.InvariantCulture)}{_positionUnit}";
		}

		private static void SetElementTopPositionValue(XElement element, double valueInPt, string @namespace)
		{
			var topPositionElement = GetTopPositionElement(element, @namespace);
			topPositionElement.Value = $"{valueInPt.ToString("0.00", CultureInfo.InvariantCulture)}{_positionUnit}";
		}

		private static XElement GetLeftPositionElement(XElement element, string @namespace)
		{
			return element.GetSingleChildElement(_leftPositionElementName, @namespace);
		}

		private static XElement GetTopPositionElement(XElement element, string @namespace)
		{
			return element.GetSingleChildElement(_topPositionElementName, @namespace);
		}
	}
}
