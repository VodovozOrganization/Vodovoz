using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.RDL.Elements;
using Vodovoz.Reports.Editing.ModifierActions;

namespace Vodovoz.Reports.Editing.Modifiers
{
	public class ProfitabilityReportModifier : ReportModifierBase
	{
		//Первая группа по левому краю
		//Вторая группа по центру
		//Третья группа по правому краю

		private const string TableName = "TableProfitability";

		//private const string ItemHeaderName = "TextboxItemHeader";
		private const string ItemDataName = "TextboxItemData";

		private const string GroupLevel1Name = "group1";
		private const string GroupLevel2Name = "group2";
		//Группировка третьего уровня на уровне RDL не нужна, она применяется в запросе, группируя исходные данные

		public void Setup(IEnumerable<GroupingType> groupings)
		{
			var actions = new List<ModifierAction>();

			var groupingActions = GetGroupingActions(groupings);
			actions.AddRange(groupingActions);

			var itemDataActions = GetItemDataActions(groupings);
			actions.AddRange(itemDataActions);

			foreach(var action in actions)
			{
				AddAction(action);
			}
		}

		private IEnumerable<ModifierAction> GetGroupingActions(IEnumerable<GroupingType> groupings)
		{
			var groupCount = groupings.Count();
			switch(groupCount)
			{
				case 1: yield break;
				case 2:
					var action = GetFirstLevelAction(groupings.First());
					yield return action;
					break;
				case 3:
					var firstAction = GetFirstLevelAction(groupings.First());
					var secondAction = GetSecondLevelAction(groupings.Skip(1).First());
					yield return firstAction;
					yield return secondAction;
					break;
				default:
					throw new InvalidOperationException("Выбрано не поддерживаемое количество группировок");
			}
		}

		private IEnumerable<ModifierAction> GetItemDataActions(IEnumerable<GroupingType> groupings)
		{
			var groupCount = groupings.Count();
			switch(groupCount)
			{
				case 1:
					var firstAction1 = GetItemTextboxUpdateAction(groupings.First());
					yield return firstAction1;
					break;
				case 2:
					var firstAction2 = GetItemTextboxUpdateAction(groupings.First(), GroupLevel1Name);
					var secondAction2 = GetItemTextboxUpdateAction(groupings.Skip(1).First(), itsLastLevel: true);
					yield return firstAction2;
					yield return secondAction2;
					break;
				case 3:
					var firstAction3 = GetItemTextboxUpdateAction(groupings.First(), GroupLevel1Name);
					var secondAction3 = GetItemTextboxUpdateAction(groupings.Skip(1).First(), GroupLevel2Name);
					var thirdAction3 = GetItemTextboxUpdateAction(groupings.Skip(2).First(), itsLastLevel: true);
					yield return firstAction3;
					yield return secondAction3;
					yield return thirdAction3;
					break;
				default:
					throw new InvalidOperationException("Выбрано не поддерживаемое количество группировок");
			}
		}

		private NewTableGroupWithCellsFromDetails GetFirstLevelAction(GroupingType groupingType)
		{
			var groupExpression = GetGroupExpression(groupingType);
			var groupModifyAction = new NewTableGroupWithCellsFromDetails(TableName, groupExpression);
			groupModifyAction.NewGroupName = GroupLevel1Name;
			groupModifyAction.GroupCellsStyle = GetFirstLevelGroupStyle();
			return groupModifyAction;
		}

		private NewTableGroupWithCellsFromDetails GetSecondLevelAction(GroupingType groupingType)
		{
			var groupExpression = GetGroupExpression(groupingType);
			var groupModifyAction = new NewTableGroupWithCellsFromDetails(TableName, groupExpression);
			groupModifyAction.NewGroupName = GroupLevel2Name;
			groupModifyAction.AfterGroup = GroupLevel1Name;
			groupModifyAction.GroupCellsStyle = GetSecondLevelGroupStyle();
			return groupModifyAction;
		}

		private FindAndModifyTextbox GetItemTextboxUpdateAction(GroupingType groupType, string groupName = null, bool itsLastLevel = false)
		{
			var textBoxName = groupName == null ? ItemDataName : $"{ItemDataName}_{groupName}";
			var action = new FindAndModifyTextbox(textBoxName, (textBox) =>
			{
				textBox.Value = GetItemDataExpression(groupType);
				switch(groupName)
				{
					case GroupLevel1Name:
						textBox.Style = GetFirstLevelGroupStyle();
						break;
					case GroupLevel2Name:
						textBox.Style = GetSecondLevelGroupStyle();
						break;
					default:
						break;
				}
				if(itsLastLevel)
				{
					textBox.Style.TextAlign = "Right";
				}
			});

			return action;
		}

		private string GetItemDataExpression(GroupingType groupType)
		{
			switch(groupType)
			{
				case GroupingType.Order: return "='Заказ №' + {order_id}";
				case GroupingType.Counterparty: return "={counterparty}";
				case GroupingType.Subdivision: return "={author_subdivision}";
				case GroupingType.DeliveryDate: return "={delivery_date}";
				case GroupingType.RouteList: return "='МЛ №' + {route_list}";
				case GroupingType.Nomenclature: return "={nomenclature_name}";
				case GroupingType.NomenclatureGroup1: return "={nomen_group_level_1_name}";
				case GroupingType.NomenclatureGroup2: return "={nomen_group_level_2_name}";
				case GroupingType.NomenclatureGroup3: return "={nomen_group_level_3_name}";
				default:
					throw new InvalidOperationException("Неизвестная группировка");
			}
		}

		private string GetGroupField(GroupingType groupType)
		{
			switch(groupType)
			{
				case GroupingType.Order : return "{order_id}";
				case GroupingType.Counterparty: return "{counterparty}";
				case GroupingType.Subdivision: return "{author_subdivision_id}";
				case GroupingType.DeliveryDate: return "{delivery_date}";
				case GroupingType.RouteList: return "{route_list}";
				case GroupingType.Nomenclature: return "{nomenclature_id}";
				case GroupingType.NomenclatureGroup1: return "{nomen_group_level_1_id}";
				case GroupingType.NomenclatureGroup2: return "{nomen_group_level_2_id}";
				case GroupingType.NomenclatureGroup3: return "{nomen_group_level_3_id}";
				default:
					throw new InvalidOperationException("Неизвестная группировка");
			}
		}

		private string[] GetGroupExpression(GroupingType groupType)
		{
			var field = GetGroupField(groupType);
			return new[] { $"={field}" } ;
		}

		private Style GetFirstLevelGroupStyle()
		{
			var style = GetBaseStyleForGrouping();
			style.TextAlign = "Left";
			return style;
		}

		private Style GetSecondLevelGroupStyle()
		{
			var style = GetBaseStyleForGrouping();
			style.TextAlign = "Center";
			return style;
		}

		private Style GetBaseStyleForGrouping()
		{
			var style = new Style();
			style.BorderStyle = new BorderColorStyleWidth();
			style.BorderStyle.Default = "Solid";
			style.BorderStyle.Left = "Solid";
			style.BorderStyle.Right= "Solid";
			style.BorderStyle.Top= "Solid";
			style.BorderStyle.Bottom= "Solid";
			style.FontWeight = "Bold";
			style.FontSize = "6pt";
			style.VerticalAlign = "Middle";
			style.Format = "0.00";
			return style;
		}
	}

	public enum GroupingType
	{
		Order,
		Counterparty,
		Subdivision,
		DeliveryDate,
		RouteList,
		Nomenclature,
		NomenclatureGroup1,
		NomenclatureGroup2,
		NomenclatureGroup3
	}
}
