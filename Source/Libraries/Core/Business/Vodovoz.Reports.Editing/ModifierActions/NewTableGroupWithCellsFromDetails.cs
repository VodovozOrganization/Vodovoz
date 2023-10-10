using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.RDL;
using Vodovoz.RDL.Elements;
using Vodovoz.RDL.Utilities;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	//В идеале действия разбить на разные классы, чтобы можно было гибко
	//настраивать их и комбинировать с другими действиями 

	/// <summary>
	/// Добавляет новую группировку в таблицу, 
	/// ячейки копируются из строк детализации,
	/// формулы копируются из строк опеределнных в <see cref="ExpressionRowProvider"/>,
	/// стиль ячеек копируется из детализации, если не определен свой
	/// </summary>
	public class NewTableGroupWithCellsFromDetails : ModifierAction
	{
		private readonly string _tableName;
		private readonly SourceRowProvider _sourceRowProvider;
		private readonly ExpressionRowProvider _expressionRowProvider;
		private readonly IEnumerable<string> _groupExpressions;
		private readonly SourceRowProvider _detailsProvider;
		private TableGroup _newGroup;
		private int _groupNameSuffixCounter;
		private readonly int? _sortByColumnIndex;
		private readonly SortByTypeDirection? _sortByTypeDirection;

		public NewTableGroupWithCellsFromDetails(
			string tableName, 
			SourceRowProvider sourceRowProvider, 
			ExpressionRowProvider expressionRowProvider, 
			IEnumerable<string> groupExpressions,
			int? sortByColumnIndex = null,
			SortByTypeDirection? sortByTypeDirection = null)
		{
			if(string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"'{nameof(tableName)}' cannot be null or whitespace.", nameof(tableName));
			}

			if(groupExpressions is null)
			{
				throw new ArgumentNullException(nameof(groupExpressions));
			}

			if(groupExpressions is null || !groupExpressions.Any())
			{
				throw new ArgumentException($"Должны быть указаны выражения для группировки", nameof(groupExpressions));
			}

			_tableName = tableName;
			_sourceRowProvider = sourceRowProvider ?? throw new ArgumentNullException(nameof(sourceRowProvider));
			_expressionRowProvider = expressionRowProvider ?? throw new ArgumentNullException(nameof(expressionRowProvider));
			_groupExpressions = groupExpressions;
			_sortByColumnIndex = sortByColumnIndex;
			_sortByTypeDirection = sortByTypeDirection;
			_newGroup = new TableGroup();
		}

		public string NewGroupName { get; set; }
		public string AfterGroup { get; set; }
		public Style GroupCellsStyle { get; set; }

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			SortExpression sortExpression = null;

			//Копирование строки из деталей и установка формул
			//Установка новых имен для текстовых боксов ячеек
			//Установка стиля для ячеек
			var rowDest = _sourceRowProvider.GetSourceRow(report, _tableName);
			var rowExpressionsSource = _expressionRowProvider.GetExpressionRow(report, _tableName);
			for(int i = 0; i < rowDest.Cells.Count; i++)
			{
				var destCell = rowDest.Cells[i];
				var sourceCell = rowExpressionsSource.Cells[i];

				var destTextbox = destCell.ReportItems.Textbox;
				var sourceTextbox = sourceCell.ReportItems.Textbox;
				if(sourceTextbox?.Value != null && sourceTextbox.Value.StartsWith("="))
				{
					destTextbox.Value = sourceTextbox.Value;
				}

				if(_sortByColumnIndex != null 
					&& i == _sortByColumnIndex.Value
					&& !string.IsNullOrWhiteSpace(sourceTextbox?.Value))
				{
					sortExpression = new SortExpression(
						sourceTextbox.Value,
						_sortByTypeDirection ?? SortByTypeDirection.Ascending);
				}

				if(GroupCellsStyle != null)
				{
					destTextbox.Style = GroupCellsStyle;
				}

				destTextbox.Name = $"{destTextbox.Name}_{NewGroupName}";
			}

			//Установка новой строки в заголовок группировки
			var groupHeader = new Header();
			groupHeader.TableRows.Add(rowDest);
			_newGroup.Header = groupHeader;

			//Определяем значения группировки
			UpdateGroupName(report, @namespace);
			var grouping = new Grouping();
			grouping.Name = NewGroupName;
			grouping.PageBreakAtStart = false;
			grouping.PageBreakAtEnd = false;
			foreach(var groupExpression in _groupExpressions)
			{
				grouping.GroupExpressions.Add(groupExpression);
			}

			_newGroup.Grouping = grouping;

			if(sortExpression != null)
			{
				var sorting = new Sorting();
				var sortBy = new SortBy();

				sortBy.AddSortExpression(sortExpression);

				sorting.SortBy.Add(sortBy);
				_newGroup.Sorting = sorting;
			}

			//Добавляем группировку в таблицу
			AddGroupToTable(report, _newGroup, @namespace);
		}

		private void AddGroupToTable(XDocument report, TableGroup tableGroup, string @namespace)
		{
			var table = report.GetTable(_tableName, @namespace);

			var afterGrouping = table.GetGroupingOrNull(AfterGroup, @namespace);
			if(afterGrouping != null)
			{
				var tableGroupElement = tableGroup.ToXElement<TableGroup>(@namespace);
				tableGroupElement.RemoveAttributes();
				afterGrouping.Parent.AddAfterSelf(tableGroupElement);
			}
			else
			{
				var tableGroupsElement = table.Element(XName.Get(nameof(TableGroups), @namespace));
				if(tableGroupsElement == null)
				{
					var tableGroups = new TableGroups();
					tableGroups.TableGroup.Add(tableGroup);
					tableGroupsElement = tableGroups.ToXElement<TableGroups>(@namespace);
					tableGroupsElement.RemoveAttributes();
					table.Add(tableGroupsElement);
				}
				else
				{
					var tableGroupElement = tableGroup.ToXElement<TableGroup>();
					tableGroupsElement.Add(tableGroupElement);
				}
			}
		}

		private void UpdateGroupName(XDocument report, string @namespace)
		{
			bool hasGrouping;
			do
			{
				string groupName = NewGroupName;
				if(_groupNameSuffixCounter > 0)
				{
					groupName = $"{NewGroupName}_{_groupNameSuffixCounter}";
				}

				hasGrouping = report.GetTable(_tableName, @namespace)
					.HasGrouping(groupName, @namespace);

				if(!hasGrouping)
				{
					NewGroupName = groupName;
					break;
				}
				else
				{
					_groupNameSuffixCounter++;
				}
			} while(!hasGrouping);
		}
	}
}
