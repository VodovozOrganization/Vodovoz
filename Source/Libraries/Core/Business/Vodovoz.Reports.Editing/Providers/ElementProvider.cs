using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Vodovoz.Reports.Editing.Providers
{
	public static partial class ElementProvider
	{
		public static XElement GetTable(this XContainer container, string tableName, string @namespace)
		{
			var tables = container.Descendants(XName.Get("Table", @namespace));
			if(!tables.Any())
			{
				throw new InvalidOperationException("В контейнере отсутствуют таблицы");
			}

			var matchedTables = tables.Where(x => x.Attribute(XName.Get("Name")).Value == tableName);
			if(!matchedTables.Any())
			{
				throw new InvalidOperationException($"В контейнере отсутствуют таблицы с именем {tableName}");
			}

			if(matchedTables.Count() > 1)
			{
				throw new InvalidOperationException($"В контейнере присутствуют несколько таблиц с именем {tableName}");
			}
			var table = matchedTables.First();
			return table;
		}

		public static void InsertTable(this XContainer container, XElement table, string @namespace)
		{
			container.InsertTableIntoReportItems(table, @namespace);
		}

		public static void RenameTable(this XContainer container, string oldName, string newName, string @namespace)
		{
			var element = container.GetTable(oldName, @namespace);
			element.Attribute("Name").Value = newName;
		}

		private static void InsertTableIntoReportItems(this XContainer container, XElement table, string @namespace)
		{
			var newTableName = table.Attribute("Name").Value;

			var existingTable = container
				.Descendants(XName.Get("Table", @namespace))
				.Where(x => x.Attribute(XName.Get("Name")).Value == newTableName)
				.FirstOrDefault();

			if(!(existingTable is null))
			{
				throw new InvalidOperationException("В контейнере уже присутствует таблица с указанным именем!");
			}

			var reportItem = container.Descendants(XName.Get("ReportItems", @namespace)).FirstOrDefault();
			reportItem.Add(table);
		}

		public static XElement GetChildElement(this XContainer container, string element, string @namespace)
		{
			if(container is null)
			{
				throw new ArgumentNullException(nameof(container));
			}
			var childElements = container.Elements(XName.Get(element, @namespace));

			if(!childElements.Any())
			{
				var errorMessage = $"Элемент \"{element}\" не найден";
				throw new InvalidOperationException(errorMessage);
			}

			if(childElements.Count() > 1)
			{
				var errorMessage = $"Найдено более одного элемента \"{element}\"";
				throw new InvalidOperationException(errorMessage);
			}

			return childElements.First();
		}

		public static XElement GetTextbox(this XContainer container, string elementNameAttributeValue, string @namespace)
		{
			return container.GetElement("Textbox", elementNameAttributeValue, @namespace);
		}

		public static XElement GetRectangle(this XContainer container, string elementNameAttributeValue, string @namespace)
		{
			return container.GetElement("Rectangle", elementNameAttributeValue, @namespace);
		}

		private static XElement GetElement(this XContainer container, string elementLocalName, string elementNameAttributeValue, string @namespace)
		{
			var elements = container.Descendants(XName.Get(elementLocalName, @namespace));
			if(!elements.Any())
			{
				throw new InvalidOperationException($"В контейнере отсутствуют элементы \"{elementLocalName}\"");
			}

			var matchedElements = elements.Where(x => x.Attribute(XName.Get("Name")).Value == elementNameAttributeValue);
			if(!matchedElements.Any())
			{
				throw new InvalidOperationException($"В контейнере отсутствуют элементы \"{elementLocalName}\" с именем {elementNameAttributeValue}");
			}

			if(matchedElements.Count() > 1)
			{
				throw new InvalidOperationException($"В контейнере присутствуют несколько элементов \"{elementLocalName}\" с именем {elementNameAttributeValue}");
			}
			var element = matchedElements.First();
			return element;
		}

		public static bool HasGrouping(this XContainer container, string groupName, string @namespace)
		{
			var grouping = container.GetGroupingOrNull(groupName, @namespace);
			return grouping != null;
		}

		public static XElement GetGroupingOrNull(this XContainer container, string groupName, string @namespace)
		{
			var groupings = container.Descendants(XName.Get("Grouping", @namespace));
			if(!groupings.Any())
			{
				return null;
			}

			var matchedGroupings = groupings.Where(x => x.Attribute(XName.Get("Name")).Value == groupName);
			if(!matchedGroupings.Any())
			{
				return null;
			}

			if(matchedGroupings.Count() > 1)
			{
				throw new InvalidOperationException($"В контейнере присутствуют несколько группировок с именем {groupName}");
			}

			return matchedGroupings.First();
		}

		public static XElement GetDetails(this XContainer container, string @namespace)
		{
			var details = container.Descendants(XName.Get("Details", @namespace));
			if(!details.Any())
			{
				ThrowMissingElementException("Details");
			}
			return details.First();
		}

		public static XElement GetFooter(this XContainer container, string @namespace)
		{
			var footers = container.Descendants(XName.Get("Footer", @namespace));
			if(!footers.Any())
			{
				ThrowMissingElementException("Footer");
			}
			return footers.First();
		}

		public static XElement GetHeader(this XContainer container, string @namespace)
		{
			var headers = container.Descendants(XName.Get("Header", @namespace));
			if(!headers.Any())
			{
				ThrowMissingElementException("Header");
			}
			return headers.First();
		}

		public static IEnumerable<XElement> GetTableRows(this XContainer container, string @namespace)
		{
			var rows = container.Descendants(XName.Get("TableRows", @namespace));
			return rows;
		}

		public static IEnumerable<XElement> GetTableColumns(this XContainer container, string @namespace)
		{
			return container.Descendants(XName.Get("TableColumns", @namespace));
		}

		public static IEnumerable<XElement> GetRowCells(this XContainer container, string @namespace)
		{
			return container.Descendants(XName.Get("TableCells", @namespace));
		}

		public static int GetTextBoxColumnIndex(this XContainer container, string textBoxName, string @namespace)
		{
			var rowsSets = container.GetTableRows(@namespace);

			foreach(var rowSet in rowsSets)
			{
				foreach(var row in rowSet.Elements())
				{
					var cells = row.GetRowCells(@namespace).Elements();

					var cellsCount = cells.Count();

					for(var i = 0; i < cellsCount; i++)
					{
						var element = cells.ElementAt(i);

						var reportItems = element.Elements().FirstOrDefault();

						if(reportItems is null)
						{
							continue;
						}

						var textbox = reportItems.Elements().FirstOrDefault();

						if(textbox is null)
						{
							continue;
						}

						if(textbox.Attributes().FirstOrDefault(x => x.Name == "Name")?.Value == textBoxName)
						{
							return i;
						}
					}
				}
			}

			return -1;
		}

		private static void ThrowMissingElementException(string element)
		{
			throw new InvalidOperationException($"В контейнере отсутствуют элементы {element}");
		}
	}
}
