using System;
using System.Collections.Generic;
using Vodovoz.Reports.Editing.ModifierActions;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.Modifiers
{
	public class WaterCarLoadDocumentModifier : ReportModifierBase
	{
		private const string _dataTableName = "TableData";
		private const string _dataWithoutQrTableName = "TableDataWithoutQr";
		private const string _tearOffCouponTableName = "TableTearOffCoupon";
		private const string _loadEndTextboxName = "TextboxLoadEnd";
		private const string _loadEndQrRectangleName = "RightQrRectangle";
		private const string _confirmationTableName = "TableConfirmation";
		private const string _infoTextboxName = "TextboxInfo";

		private const double _dataTableHeightInPt = 70;
		private const double _dataWithoutQrTableHeightInPt = 70;
		private const double _tearOffCouponTableHeightInPt = 90;
		private const double _qrRectangleHeightInPt = 120;

		public void Setup(IEnumerable<int> orderIds, int tearOffCouponsCount)
		{
			if(orderIds is null)
			{
				throw new ArgumentNullException(nameof(orderIds));
			}

			AddActions(InsertDataTablesWithQr(orderIds));
			AddActions(InsertTearOffCouponTables(tearOffCouponsCount));
			AddActions(InsertDataTablesWithoutQr(orderIds));

			AddAction(RemoveTableAction(_dataTableName));
			AddAction(RemoveTableAction(_dataWithoutQrTableName));
		}

		private static IEnumerable<ModifierAction> InsertDataTablesWithQr(IEnumerable<int> orderIds)
		{
			var counter = 0;

			var actions = new List<ModifierAction>();

			foreach(var orderId in orderIds)
			{
				var verticalOffset = counter * _dataTableHeightInPt;
				var newTableName = $"{_dataTableName}_qr_{orderId}";

				actions.AddRange(CopyTableAndMoveDownActions(_dataTableName, newTableName, verticalOffset));
				actions.Add(AddOrderEqualTableFilterAction(newTableName, orderId.ToString()));

				counter++;
			}

			if(counter > 0)
			{
				var offsetForNextElements = counter * _dataTableHeightInPt;

				actions.AddRange(GetDataTableWithCommonOrders(orderIds, $"{_dataTableName}_qr_common", offsetForNextElements));

				actions.Add(MoveTextboxDownAction(_loadEndTextboxName, offsetForNextElements));
				actions.Add(MoveRectangleDownAction(_loadEndQrRectangleName, offsetForNextElements));
				actions.Add(MoveTableDownAction(_tearOffCouponTableName, offsetForNextElements));
				actions.Add(MoveTableDownAction(_dataWithoutQrTableName, offsetForNextElements));
				actions.Add(MoveTableDownAction(_confirmationTableName, offsetForNextElements));
				actions.Add(MoveTextboxDownAction(_infoTextboxName, offsetForNextElements));
			}

			return actions;
		}

		private static IEnumerable<ModifierAction> InsertTearOffCouponTables(int totalCount)
		{
			var actions = new List<ModifierAction>();

			if(totalCount < 2)
			{
				return actions;
			}

			for(int i = 1; i < totalCount; i++)
			{
				var newTearOffCouponTableName = $"{_tearOffCouponTableName}_{i}";
				var verticalOffset = _tearOffCouponTableHeightInPt * i;

				actions.Add(CopyTableAction(_tearOffCouponTableName, newTearOffCouponTableName));
				actions.Add(MoveTableDownAction(newTearOffCouponTableName, verticalOffset));
			}

			var offsetForNextElements = (totalCount - 1) * _tearOffCouponTableHeightInPt;

			actions.Add(MoveTableDownAction(_dataWithoutQrTableName, offsetForNextElements));
			actions.Add(MoveTableDownAction(_confirmationTableName, offsetForNextElements));
			actions.Add(MoveTextboxDownAction(_infoTextboxName, offsetForNextElements));

			return actions;
		}

		private static IEnumerable<ModifierAction> InsertDataTablesWithoutQr(IEnumerable<int> orderIds)
		{
			var counter = 0;

			var actions = new List<ModifierAction>();

			foreach(var orderId in orderIds)
			{
				var verticalOffset = counter * _dataWithoutQrTableHeightInPt;
				var newTableName = $"{_dataWithoutQrTableName}_{orderId}";

				actions.AddRange(CopyTableAndMoveDownActions(_dataWithoutQrTableName, newTableName, verticalOffset));
				actions.Add(AddOrderEqualTableFilterAction(newTableName, orderId.ToString()));

				counter++;
			}

			if(counter > 0)
			{
				var offsetForNextElements = counter * _dataWithoutQrTableHeightInPt;

				actions.AddRange(GetDataTableWithCommonOrders(orderIds, $"{_dataWithoutQrTableName}_common", offsetForNextElements, true));

				actions.Add(MoveTableDownAction(_confirmationTableName, offsetForNextElements));
				actions.Add(MoveTextboxDownAction(_infoTextboxName, offsetForNextElements));
			}

			return actions;
		}

		private static IEnumerable<ModifierAction> GetDataTableWithCommonOrders(IEnumerable<int> orderIds, string newTableName,
			double verticalOffset, bool copyFromTableWithoutQr = false)
		{
			var actions = new List<ModifierAction>();

			var sourceTableName = copyFromTableWithoutQr ? _dataWithoutQrTableName : _dataTableName;

			actions.AddRange(CopyTableAndMoveDownActions(sourceTableName, newTableName, verticalOffset));

			foreach(var orderId in orderIds)
			{
				actions.Add(AddOrderNotEqualTableFilterAction(newTableName, orderId.ToString()));
			}

			return actions;
		}

		private static IEnumerable<ModifierAction> CopyTableAndMoveDownActions(string sourceTableName, string newTableName, double verticalOffset)
		{
			var actions = new List<ModifierAction>
			{
				CopyTableAction(sourceTableName, newTableName),
				MoveTableDownAction(newTableName, verticalOffset),
			};

			return actions;
		}

		private static ModifierAction CopyTableAction(string sourceTableName, string newTableName)
		{
			return new CopyTable(sourceTableName, newTableName);
		}

		private static ModifierAction RemoveTableAction(string tableName)
		{
			return new RemoveTable(tableName);
		}

		private static ModifierAction SetTablePositionAction(string tableName, double leftPositionInPt, double topPositionInPt)
		{
			return new SetTablePosition(tableName, leftPositionInPt, topPositionInPt);
		}

		private static ModifierAction MoveTableDownAction(string tableName, double offsetInPt)
		{
			return new MoveElementDown(tableName, ElementType.Table, offsetInPt);
		}

		private static ModifierAction MoveTextboxDownAction(string textboxName, double offsetInPt)
		{
			return new MoveElementDown(textboxName, ElementType.Textbox, offsetInPt);
		}

		private static ModifierAction MoveRectangleDownAction(string rectangleName, double offsetInPt)
		{
			return new MoveElementDown(rectangleName, ElementType.Rectangle, offsetInPt);
		}

		private static ModifierAction RenameTableAction(string tableOldName, string tableNewName)
		{
			return new RenameTable(tableOldName, tableNewName);
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

		//private static ModifierAction SetTableHeaderAction(string tableName, string expression)
		//{

		//}
	}
}
