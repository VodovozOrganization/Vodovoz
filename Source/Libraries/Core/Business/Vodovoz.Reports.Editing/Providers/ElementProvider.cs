using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Vodovoz.Reports.Editing.Providers
{
	public static class ElementProvider
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

		public static XElement GetTextbox(this XContainer container, string textBoxName, string @namespace)
		{
			var textBoxes = container.Descendants(XName.Get("Textbox", @namespace));
			if(!textBoxes.Any())
			{
				throw new InvalidOperationException("В контейнере отсутствуют Textbox");
			}

			var matchedTextBoxes = textBoxes.Where(x => x.Attribute(XName.Get("Name")).Value == textBoxName);
			if(!matchedTextBoxes.Any())
			{
				throw new InvalidOperationException($"В контейнере отсутствуют Textbox с именем {textBoxName}");
			}

			if(matchedTextBoxes.Count() > 1)
			{
				throw new InvalidOperationException($"В контейнере присутствуют несколько Textbox с именем {textBoxName}");
			}
			var textBox = matchedTextBoxes.First();
			return textBox;
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
