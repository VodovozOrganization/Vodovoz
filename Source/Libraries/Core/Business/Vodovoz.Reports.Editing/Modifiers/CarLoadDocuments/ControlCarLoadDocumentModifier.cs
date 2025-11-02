using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Reports.Editing.ModifierActions;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.Modifiers
{
	public class ControlCarLoadDocumentModifier : ReportModifierBase
	{
		private const string _dataTableName = "TableData";
		private const string _confirmationTableName = "TableConfirmation";
		private const string _infoTextboxName = "TextboxInfo";

		private const double _dataTableHeightInPt = 70;

		public void Setup(IEnumerable<int> orderIds, bool isDocumentHasCommonOrders)
		{
			if(orderIds is null)
			{
				throw new ArgumentNullException(nameof(orderIds));
			}

			AddActions(InsertDataTables(orderIds, isDocumentHasCommonOrders));

			AddAction(RemoveTableAction(_dataTableName));
		}

		private static IEnumerable<ModifierAction> InsertDataTables(IEnumerable<int> orderIds, bool isDocumentHasCommonOrders)
		{
			var tablesCount = orderIds.Count();

			var actions = new List<ModifierAction>();

			for(var i = 0; i < tablesCount; i++)
			{
				var orderId = orderIds.ElementAt(i);
				var verticalOffset = i * _dataTableHeightInPt;
				var newTableName = $"{_dataTableName}_{orderId}";

				actions.AddRange(CopyTableAndMoveVerticallyActions(_dataTableName, newTableName, verticalOffset));
				actions.Add(AddOrderEqualTableFilterAction(newTableName, orderId.ToString()));
			}

			var offsetForNextElements = tablesCount > 0 ? tablesCount * _dataTableHeightInPt : 0;

			if(isDocumentHasCommonOrders)
			{
				var commonTableName = $"{_dataTableName}_common";
				actions.AddRange(GetDataTableWithCommonOrdersActions(orderIds, commonTableName, offsetForNextElements));
			}

			offsetForNextElements =
				isDocumentHasCommonOrders
				? offsetForNextElements
				: offsetForNextElements - _dataTableHeightInPt;

			actions.Add(MoveTableVerticallyAction(_confirmationTableName, offsetForNextElements));
			actions.Add(MoveTextboxVerticallyAction(_infoTextboxName, offsetForNextElements));

			return actions;
		}

		private static IEnumerable<ModifierAction> GetDataTableWithCommonOrdersActions(IEnumerable<int> orderIds, string newTableName,
			double verticalOffset)
		{
			var actions = new List<ModifierAction>();

			var sourceTableName = _dataTableName;

			actions.AddRange(CopyTableAndMoveVerticallyActions(sourceTableName, newTableName, verticalOffset));

			foreach(var orderId in orderIds)
			{
				actions.Add(AddOrderNotEqualTableFilterAction(newTableName, orderId.ToString()));
			}

			return actions;
		}

		private static IEnumerable<ModifierAction> CopyTableAndMoveVerticallyActions(string sourceTableName, string newTableName, double verticalOffset)
		{
			var actions = new List<ModifierAction>
			{
				CopyTableAction(sourceTableName, newTableName),
				MoveTableVerticallyAction(newTableName, verticalOffset),
			};

			return actions;
		}

		private static ModifierAction CopyTableAction(string sourceTableName, string newTableName)
		{
			return new CopyElement(ElementType.Table, sourceTableName, newTableName);
		}

		private static ModifierAction RemoveTableAction(string tableName)
		{
			return new RemoveElement(ElementType.Table, tableName);
		}

		private static ModifierAction MoveTableVerticallyAction(string tableName, double offsetInPt)
		{
			return new MoveElementVertically(tableName, ElementType.Table, offsetInPt);
		}

		private static ModifierAction MoveTextboxVerticallyAction(string textboxName, double offsetInPt)
		{
			return new MoveElementVertically(textboxName, ElementType.Textbox, offsetInPt);
		}

		private static ModifierAction AddOrderEqualTableFilterAction(string tableName, string value)
		{
			return AddTableFilterAction(tableName, "={order_id}", "Equal", value);
		}

		private static ModifierAction AddOrderNotEqualTableFilterAction(string tableName, string value)
		{
			return AddTableFilterAction(tableName, "={order_id}", "NotEqual", value);
		}

		private static ModifierAction AddTableFilterAction(string tableName, string expression, string @operator, string value)
		{
			return new AddTableFilter(tableName, expression, @operator, value);
		}
	}
}
