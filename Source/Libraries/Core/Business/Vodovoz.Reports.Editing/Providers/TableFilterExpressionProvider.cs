using System.Xml.Linq;

namespace Vodovoz.Reports.Editing.Providers
{
	public static class TableFilterExpressionProvider
	{
		private const string _filtersElementName = "Filters";

		public static void AddTableFilter(this XContainer container, string tableName,
			string expression, string @operator, string value, string @namespace)
		{
			var table = ElementProvider.GetTable(container, tableName, @namespace);

			var filters = ElementProvider.GetChildElement(table, _filtersElementName, @namespace);

			if(filters is null)
			{
				filters = new XElement(_filtersElementName);
				table.Add(filters);
			}

			filters.Add(CreateFilterElement(expression, @operator, value));
		}

		private static XElement CreateFilterElement(string expression, string @operator, string value)
		{
			var filterExpressionElement = new XElement("FilterExpression", expression);
			var operatorElement = new XElement("Operator", @operator);
			var filterValuesElement = new XElement("FilterValues", new XElement("FilterValue", value));

			var filterElement = new XElement(
				"Filter",
				filterExpressionElement,
				operatorElement,
				filterValuesElement);

			return filterElement;
		}
	}
}
