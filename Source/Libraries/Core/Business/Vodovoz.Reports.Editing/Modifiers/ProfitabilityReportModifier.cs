using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.RDL.Elements;
using Vodovoz.Reports.Editing.ModifierActions;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.Modifiers
{
	public class ProfitabilityReportModifier : ReportModifierBase
	{
		private const string _tableName = "TableSales";

		private const string _itemDataName = "TextboxItemData";
		private const string _marginPercentTextboxName = "TextboxMarginPercent";

		private const string _groupLevel1Name = "group1";
		private const string _groupLevel2Name = "group2";
		private const string _groupLevel3Name = "group3";

		private const string _autoTypeHeaderTextBox = "TextboxAutoType";
		private const string _autoOwnerTypeHeaderTextBox = "TextboxAutoOwnerType";
		private const string _itemDataHeaderTextBox = "TextboxItemHeader";

		private const int _itemDataColumnIncreasedWidth = 190;

		private const int _profitabilityPercentColumnId = 8;

		private readonly ExpressionRowProvider _expressionRowProvider;
		private readonly SourceRowProvider _sourceRowProvider;

		private bool _isGroupingByRouteListOnly;

		public ProfitabilityReportModifier()
		{
			_expressionRowProvider = new FooterExpressionRowProvider();
			_sourceRowProvider = new DetailsSourceRowProvider();
		}

		public static int NoMarginPercentValue = -999999;

		public void Setup(IEnumerable<GroupingType> groupings, bool isGroupingByRouteListOnly)
		{
			_isGroupingByRouteListOnly = isGroupingByRouteListOnly;

			var groupingActions = GetGroupingActions(groupings);
			foreach (var action in groupingActions)
			{
				AddAction(action);
			}

			var itemDataActions = GetItemDataActions(groupings);
			foreach(var action in itemDataActions)
			{
				AddAction(action);
			}

			var removeDetailsAction = new RemoveDetails(_tableName);
			AddAction(removeDetailsAction);

			var removeFooterAction = new RemoveFooter(_tableName);
			AddAction(removeFooterAction);

			if(_isGroupingByRouteListOnly)
			{
				AddAction(GetModifyMarginPercenTexboxAction());
				return;
			}

			var setColumnWidthAction = new SetColumnWidth(_tableName, _itemDataHeaderTextBox, _itemDataColumnIncreasedWidth);
			AddAction(setColumnWidthAction);

			var removeAutoTypeColumn = new RemoveColumn(_tableName, _autoTypeHeaderTextBox);
			AddAction(removeAutoTypeColumn);

			var removeAutoOwnerTypeColumn = new RemoveColumn(_tableName, _autoOwnerTypeHeaderTextBox);
			AddAction(removeAutoOwnerTypeColumn);
		}

		private ModifierAction GetModifyMarginPercenTexboxAction() =>
			new FindAndModifyTextbox(
					$"{_marginPercentTextboxName}_group1",
					textbox =>
					{
						textbox.Value =
							"=Round(Iif(Sum({total_price})=0,"
							+ NoMarginPercentValue +
							", Sum({profitability})*100/Sum({total_price})), 2)";
					});


		private IEnumerable<ModifierAction> GetGroupingActions(IEnumerable<GroupingType> groupings)
		{
			var groupCount = groupings.Count();

			switch(groupCount)
			{
				case 1:
					yield return GetFirstLevelAction(groupings.First(), groupCount);
					break;
				case 2:
					yield return GetFirstLevelAction(groupings.First(), groupCount);
					yield return GetSecondLevelAction(groupings.Skip(1).First(), groupCount);
					break;
				case 3:
					yield return GetFirstLevelAction(groupings.First(), groupCount);
					yield return GetSecondLevelAction(groupings.Skip(1).First(), groupCount);
					yield return GetThirdLevelAction(groupings.Skip(2).First(), groupCount);
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
					yield return GetItemTextboxUpdateAction(groupings.First(), _groupLevel1Name, groupCount);
					break;
				case 2:
					yield return GetItemTextboxUpdateAction(groupings.First(), _groupLevel1Name, groupCount);
					yield return GetItemTextboxUpdateAction(groupings.Skip(1).First(), _groupLevel2Name, groupCount);
					break;
				case 3:
					yield return GetItemTextboxUpdateAction(groupings.First(), _groupLevel1Name, groupCount);
					yield return GetItemTextboxUpdateAction(groupings.Skip(1).First(), _groupLevel2Name, groupCount);
					yield return GetItemTextboxUpdateAction(groupings.Skip(2).First(), _groupLevel3Name, groupCount);
					break;
				default:
					throw new InvalidOperationException("Выбрано не поддерживаемое количество группировок");
			}
		}

		private NewTableGroupWithCellsFromDetails GetFirstLevelAction(GroupingType groupingType, int groupsCount)
		{
			var style = groupsCount == 1 ? GetLastLevelGroupStyle() : GetFirstLevelGroupStyle();
			style.Format = "# ##0.00";

			var groupExpression = GetGroupExpression(groupingType);

			NewTableGroupWithCellsFromDetails groupModifyAction;

			if(_isGroupingByRouteListOnly)
			{
				groupModifyAction = new NewTableGroupWithCellsFromDetails(
					_tableName, 
					_sourceRowProvider, 
					_expressionRowProvider, 
					groupExpression,
					_profitabilityPercentColumnId);
			}
			else
			{
				groupModifyAction = new NewTableGroupWithCellsFromDetails(_tableName, _sourceRowProvider, _expressionRowProvider, groupExpression);
			}

			groupModifyAction.NewGroupName = _groupLevel1Name;
			groupModifyAction.GroupCellsStyle = style;
			return groupModifyAction;
		}

		private NewTableGroupWithCellsFromDetails GetSecondLevelAction(GroupingType groupingType, int groupsCount)
		{
			var style = groupsCount == 2 ? GetLastLevelGroupStyle() : GetSecondLevelGroupStyle();
			style.Format = "# ##0.00";

			var groupExpression = GetGroupExpression(groupingType);
			var groupModifyAction = new NewTableGroupWithCellsFromDetails(_tableName, _sourceRowProvider, _expressionRowProvider, groupExpression);
			groupModifyAction.NewGroupName = _groupLevel2Name;
			groupModifyAction.AfterGroup = _groupLevel1Name;
			groupModifyAction.GroupCellsStyle = style;
			return groupModifyAction;
		}

		private NewTableGroupWithCellsFromDetails GetThirdLevelAction(GroupingType groupingType, int groupsCount)
		{
			var style = GetLastLevelGroupStyle();
			style.Format = "# ##0.00";

			var groupExpression = GetGroupExpression(groupingType);
			var groupModifyAction = new NewTableGroupWithCellsFromDetails(_tableName, _sourceRowProvider, _expressionRowProvider, groupExpression);
			groupModifyAction.NewGroupName = _groupLevel3Name;
			groupModifyAction.AfterGroup = _groupLevel2Name;
			groupModifyAction.GroupCellsStyle = style;
			return groupModifyAction;
		}

		private FindAndModifyTextbox GetItemTextboxUpdateAction(GroupingType groupType, string groupName, int groupsCount)
		{
			if(string.IsNullOrWhiteSpace(groupName))
			{
				throw new ArgumentException($"'{nameof(groupName)}' cannot be null or whitespace.", nameof(groupName));
			}

			var textBoxName = $"{_itemDataName}_{groupName}";
			var action = new FindAndModifyTextbox(textBoxName, (textBox) =>
			{
				textBox.Value = GetItemDataExpression(groupType);

				Style style = null;
				switch(groupName)
				{
					case _groupLevel1Name:
						style = groupsCount == 1 ? GetLastLevelGroupStyle() : GetFirstLevelGroupStyle();
						break;
					case _groupLevel2Name:
						style = groupsCount == 2 ? GetLastLevelGroupStyle() : GetSecondLevelGroupStyle();
						break;
					case _groupLevel3Name:
						style = GetLastLevelGroupStyle();
						break;
					default:
						style = textBox.Style;
						break;
				}

				if(groupType == GroupingType.DeliveryDate)
				{
					style.Format = "dd.MM.yyyy";
				}

				textBox.Style = style;
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
				case GroupingType.RouteList: return "=Iif(Fields!route_list.IsMissing, 'Без маршрутного листа', 'МЛ №' + {route_list})";
				case GroupingType.Nomenclature: return "={nomenclature_name}";
				case GroupingType.NomenclatureType: return "={nomenclature_category}";
				case GroupingType.NomenclatureGroup1: return "={nomen_group_level_1_name}";
				case GroupingType.NomenclatureGroup2: return "={nomen_group_level_2_name}";
				case GroupingType.NomenclatureGroup3: return "={nomen_group_level_3_name}";
				case GroupingType.CounterpartyType: return "={counterparty_type}";
				case GroupingType.PaymentType: return "={payment_type}";
				case GroupingType.Organization: return "={organization}";
				case GroupingType.CounterpartyClassification: return "={counterparty_classification}";
				case GroupingType.PromotionalSet: return "={promotional_set}";
				case GroupingType.CounterpartyManager: return "={sales_manager_name}";
				case GroupingType.OrderAuthor: return "={order_author_name}";
				default:
					throw new NotSupportedException("Неизвестная группировка");
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
				case GroupingType.NomenclatureType: return "{nomenclature_category}";
				case GroupingType.NomenclatureGroup1: return "{nomen_group_level_1_id}";
				case GroupingType.NomenclatureGroup2: return "{nomen_group_level_2_id}";
				case GroupingType.NomenclatureGroup3: return "{nomen_group_level_3_id}";
				case GroupingType.CounterpartyType: return "{counterparty_type}";
				case GroupingType.PaymentType: return "{payment_type}";
				case GroupingType.Organization: return "{organization}";
				case GroupingType.CounterpartyClassification: return "{counterparty_classification}";
				case GroupingType.PromotionalSet: return "{promotional_set}";
				case GroupingType.CounterpartyManager: return "{sales_manager_name}";
				case GroupingType.OrderAuthor: return "{order_author_name}";
				default:
					throw new NotSupportedException("Неизвестная группировка");
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

		private Style GetLastLevelGroupStyle()
		{
			var style = GetBaseStyleForGrouping();
			style.TextAlign = "Right";
			style.FontWeight = "Nornal";
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
			return style;
		}
	}
}
