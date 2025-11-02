using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Reports.Editing.ModifierActions;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.Modifiers
{
	public class WaterCarLoadDocumentModifier : ReportModifierBase
	{
		private const string _dataTableName = "TableData";
		private const string _tearOffCouponTableName = "TableTearOffCoupon";
		private const string _orderQrCodeName = "OrderQrCode";

		private const double _dataTableHeightInPt = 145;
		private const double _tearOffCouponTableHeightInPt = 90;
		private const double _dataTableDefaultHeaderRowHeightInPt = 25;
		private const double _dataTableWithQrHeaderRowHeightInPt = 85;

		public void Setup(IEnumerable<int> orderIds, int tearOffCouponsCount, bool isDocumentHasCommonOrders)
		{
			if(orderIds is null)
			{
				throw new ArgumentNullException(nameof(orderIds));
			}

			AddActions(InsertDataTablesWithQr(orderIds, isDocumentHasCommonOrders));
			AddActions(InsertTearOffCouponTablesActions(tearOffCouponsCount));

			AddAction(RemoveTableAction(_dataTableName));
			AddAction(RemoveQrCodeAction(_orderQrCodeName));
		}

		private static IEnumerable<ModifierAction> InsertDataTablesWithQr(IEnumerable<int> orderIds, bool isDocumentHasCommonOrders)
		{
			var tablesWithQrCount = orderIds.Count();

			var actions = new List<ModifierAction>();

			for(var i = 0; i < tablesWithQrCount; i++)
			{
				var orderId = orderIds.ElementAt(i);
				var verticalOffset = i * _dataTableHeightInPt;
				var newTableName = $"{_dataTableName}_qr_{orderId}";

				actions.AddRange(CopyTableAndMoveVerticallyActions(_dataTableName, newTableName, verticalOffset));
				actions.Add(AddOrderEqualTableFilterAction(newTableName, orderId.ToString()));
				actions.AddRange(AddQrCodeForTableAndMoveVerticallyActions(orderId, verticalOffset));
			}

			var offsetForNextElements = tablesWithQrCount > 0 ? tablesWithQrCount * _dataTableHeightInPt : 0;

			if(isDocumentHasCommonOrders)
			{
				var commonTableName = $"{_dataTableName}_qr_common";
				actions.AddRange(GetDataTableWithCommonOrdersActions(orderIds, commonTableName, offsetForNextElements));
				actions.Add(SetTableHeaderHeightAction(commonTableName, _dataTableDefaultHeaderRowHeightInPt));
			}

			offsetForNextElements =
				isDocumentHasCommonOrders
				? offsetForNextElements - (_dataTableWithQrHeaderRowHeightInPt - _dataTableDefaultHeaderRowHeightInPt)
				: offsetForNextElements - _dataTableHeightInPt;

			actions.Add(MoveTableVerticallyAction(_tearOffCouponTableName, offsetForNextElements));

			return actions;
		}

		private static IEnumerable<ModifierAction> AddQrCodeForTableAndMoveVerticallyActions(int orderId, double verticalOffset)
		{
			var actions = new List<ModifierAction>();

			var qrCodeItemName = $"{_orderQrCodeName}_{orderId}";
			var qrCodeValue = $"='CarLoadDocument;' + {{?id}} + ';{orderId}'";

			actions.AddRange(CopyQrCodeAndMoveVerticallyActions(_orderQrCodeName, qrCodeItemName, verticalOffset));
			actions.Add(SetQrCodeValueAction(qrCodeItemName, qrCodeValue));

			return actions;
		}

		private static IEnumerable<ModifierAction> InsertTearOffCouponTablesActions(int totalCount)
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
				actions.Add(MoveTableVerticallyAction(newTearOffCouponTableName, verticalOffset));
			}

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

		private static IEnumerable<ModifierAction> CopyQrCodeAndMoveVerticallyActions(string sourceQrItemName, string newQrItemName, double verticalOffset)
		{
			var actions = new List<ModifierAction>
			{
				CopyQrCodeAction(sourceQrItemName, newQrItemName),
				MoveQrCodeVerticallyAction(newQrItemName, verticalOffset),
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

		private static ModifierAction RemoveQrCodeAction(string qrItemName)
		{
			return new RemoveElement(ElementType.CustomReportItem, qrItemName);
		}

		private static ModifierAction MoveTableVerticallyAction(string tableName, double offsetInPt)
		{
			return new MoveElementVertically(tableName, ElementType.Table, offsetInPt);
		}

		private static ModifierAction MoveQrCodeVerticallyAction(string qrItemName, double offsetInPt)
		{
			return new MoveElementVertically(qrItemName, ElementType.CustomReportItem, offsetInPt);
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

		private static ModifierAction SetTableHeaderHeightAction(string tableName, double rowHeight)
		{
			return new SetTableHeaderRowHeight(tableName, rowHeight);
		}

		private static ModifierAction CopyQrCodeAction(string sourceQrItemName, string newQrItemName)
		{
			return new CopyElement(ElementType.CustomReportItem, sourceQrItemName, newQrItemName);
		}

		private static ModifierAction SetQrCodeValueAction(string qrCodeItemName, string newValue)
		{
			return new SetQrCodeValue(qrCodeItemName, newValue);
		}
	}
}
