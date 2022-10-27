using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Vodovoz.RDL.Providers;
using Vodovoz.RDL.Elements;
using Vodovoz.Reports.Editing.Providers;
using Vodovoz.RDL.Utilities;

namespace Vodovoz.Reports.Editing.TableGrouping
{
	public class TableGroupingModifier : ReportModifierBase
	{
		
	}

	public class AddNewTableGroupAction : ModifierAction
	{
		private readonly string _tableName;
		private readonly IEnumerable<string> _groupExpressions;
		private readonly DetailsProvider _detailsProvider;
		private TableGroup _newGroup;
		private int _groupNameSuffixCounter;

		public AddNewTableGroupAction(string tableName, IEnumerable<string> groupExpressions)
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
			_groupExpressions = groupExpressions;
			_detailsProvider = new DetailsProvider();
			_newGroup = new TableGroup();
		}

		public string NewGroupName { get; set; }
		public string AfterGroup { get; set; }
		public Style GroupCellsStyle { get; set; }

		public override void Modify(XDocument report)
		{
			var @namespace = GetNamespace(report);

			//Копирование строки из деталей и установка формул из подвала
			//Установка новых имен для текстовых боксов ячеек
			//Установка стиля для ячеек
			var rowDest = _detailsProvider.GetDetailsRow(report, _tableName);
			var rowExpressionsSource = _detailsProvider.GetFooterRow(report, _tableName);
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

			//Добавляем группировку в таблицу
			AddGroupToTable(report, _newGroup, @namespace);
		}

		private void AddGroupToTable(XDocument report, TableGroup tableGroup, string @namespace)
		{
			var table = report.GetTable(_tableName, @namespace);

			var afterGrouping = table.GetGroupingOrNull(AfterGroup, @namespace);
			if(afterGrouping != null)
			{
				var tableGroupElement = tableGroup.ToXElement<TableGroup>();
				afterGrouping.Parent.AddAfterSelf(tableGroupElement);
			}
			else
			{
				var tableGroupsElement = table.Element(XName.Get(nameof(TableGroups), @namespace));
				if(tableGroupsElement == null)
				{
					var tableGroups = new TableGroups();
					tableGroups.TableGroup.Add(tableGroup);
					tableGroupsElement = tableGroups.ToXElement<TableGroups>();
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

		private string GetNamespace(XDocument report)
		{
			return report.Root.Attribute("xmlns").Value;
		}

		public override IEnumerable<ValidationResult> Validate(XDocument report)
		{
			var tableColumnProvider = new TableColumnProvider(report);
			var tableColumnsCount = tableColumnProvider.GetTotalTableColumns(_tableName);
			var columnsCountAreEqual = _newGroup.Header.TableRows.All(r => r.Cells.Sum(c => c.ColSpan) == tableColumnsCount);
			if(columnsCountAreEqual)
			{
				return Enumerable.Empty<ValidationResult>();
			}
			else
			{
				return new[] { new ValidationResult($"Количество колонок (включая ColSpan) в группировке не соответствует количеству колонок в таблице {_tableName}") };
			}
		}
	}
}
