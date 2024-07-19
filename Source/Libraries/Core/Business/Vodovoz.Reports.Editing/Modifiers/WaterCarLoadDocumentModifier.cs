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
		private const string _dataWithoutQrTableName = "TableDataWithoutQr";
		private const string _tearOffCouponTableName = "TableTearOffCoupon";
		private const string _loadEndQrRectangleName = "BottomQrRectangle";
		private const string _confirmationTableName = "TableConfirmation";
		private const string _infoTextboxName = "TextboxInfo";
		private const string _orderQrCodeName = "OrderQrCode";

		private const double _dataTableHeightInPt = 70;
		private const double _dataWithoutQrTableHeightInPt = 70;
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
			AddActions(InsertDataTablesWithoutQrActions(orderIds, isDocumentHasCommonOrders));

			AddAction(RemoveTableAction(_dataTableName));
			AddAction(RemoveTableAction(_dataWithoutQrTableName));
			AddAction(RemoveQrCodeAction(_orderQrCodeName));
		}

		private static IEnumerable<ModifierAction> InsertDataTablesWithQr(IEnumerable<int> orderIds, bool isDocumentHasCommonOrders)
		{
			var tablesWithQrCount = orderIds.Count();

			var actions = new List<ModifierAction>();

			var verticalOffsetStep = _dataTableHeightInPt + _dataTableWithQrHeaderRowHeightInPt - _dataTableDefaultHeaderRowHeightInPt;

			for(var i = 0; i < tablesWithQrCount; i++)
			{
				var orderId = orderIds.ElementAt(i);
				var verticalOffset = i * verticalOffsetStep;
				var newTableName = $"{_dataTableName}_qr_{orderId}";

				actions.AddRange(CopyTableAndMoveDownActions(_dataTableName, newTableName, verticalOffset));
				actions.Add(AddOrderEqualTableFilterAction(newTableName, orderId.ToString()));

				actions.Add(SetTableHeaderHeightAction(newTableName, _dataTableWithQrHeaderRowHeightInPt));

				//Пока нет необходимости отображать QR коды для сетевых заказов
				//Когда строка будет раскомментирована, необходимо установить значение константы _dataTableWithQrHeaderRowHeightInPt = 85
				actions.AddRange(AddQrCodeForTableAndMoveDownActions(orderId, verticalOffset));

			}

			var offsetForNextElements = tablesWithQrCount * verticalOffsetStep;

			if(isDocumentHasCommonOrders)
			{
				actions.AddRange(GetDataTableWithCommonOrdersActions(orderIds, $"{_dataTableName}_qr_common", offsetForNextElements));
			}

			if(tablesWithQrCount > 0)
			{
				actions.Add(MoveRectangleDownAction(_loadEndQrRectangleName, offsetForNextElements));
				actions.Add(MoveTableDownAction(_tearOffCouponTableName, offsetForNextElements));
				actions.Add(MoveTableDownAction(_dataWithoutQrTableName, offsetForNextElements));
				actions.Add(MoveTableDownAction(_confirmationTableName, offsetForNextElements));
				actions.Add(MoveTextboxDownAction(_infoTextboxName, offsetForNextElements));
			}

			return actions;
		}

		private static IEnumerable<ModifierAction> AddQrCodeForTableAndMoveDownActions(int orderId, double verticalOffset)
		{
			var actions = new List<ModifierAction>();

			var qrCodeItemName = $"{_orderQrCodeName}_{orderId}";
			var qrCodeValue = $"='CarLoadDocument;' + {{?id}} + ';{orderId}'";

			actions.AddRange(CopyQrCodeAndMoveDownActions(_orderQrCodeName, qrCodeItemName, verticalOffset));
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
				actions.Add(MoveTableDownAction(newTearOffCouponTableName, verticalOffset));
			}

			var offsetForNextElements = (totalCount - 1) * _tearOffCouponTableHeightInPt;

			actions.Add(MoveTableDownAction(_dataWithoutQrTableName, offsetForNextElements));
			actions.Add(MoveTableDownAction(_confirmationTableName, offsetForNextElements));
			actions.Add(MoveTextboxDownAction(_infoTextboxName, offsetForNextElements));

			return actions;
		}

		private static IEnumerable<ModifierAction> InsertDataTablesWithoutQrActions(IEnumerable<int> orderIds, bool isDocumentHasCommonOrders)
		{
			var tablesWithQrCount = orderIds.Count();

			var actions = new List<ModifierAction>();

			for(var i = 0; i < tablesWithQrCount; i++)
			{
				var orderId = orderIds.ElementAt(i);
				var verticalOffset = i * _dataWithoutQrTableHeightInPt;
				var newTableName = $"{_dataWithoutQrTableName}_{orderId}";

				actions.AddRange(CopyTableAndMoveDownActions(_dataWithoutQrTableName, newTableName, verticalOffset));
				actions.Add(AddOrderEqualTableFilterAction(newTableName, orderId.ToString()));
			}

			var offsetForNextElements = tablesWithQrCount * _dataWithoutQrTableHeightInPt;

			if(isDocumentHasCommonOrders)
			{
				actions.AddRange(GetDataTableWithCommonOrdersActions(orderIds, $"{_dataWithoutQrTableName}_common", offsetForNextElements, true));
			}

			if(tablesWithQrCount > 0)
			{
				actions.Add(MoveTableDownAction(_confirmationTableName, offsetForNextElements));
				actions.Add(MoveTextboxDownAction(_infoTextboxName, offsetForNextElements));
			}

			return actions;
		}

		private static IEnumerable<ModifierAction> GetDataTableWithCommonOrdersActions(IEnumerable<int> orderIds, string newTableName,
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

		private static IEnumerable<ModifierAction> CopyQrCodeAndMoveDownActions(string sourceQrItemName, string newQrItemName, double verticalOffset)
		{
			var actions = new List<ModifierAction>
			{
				CopyQrCodeAction(sourceQrItemName, newQrItemName),
				MoveQrCodeDownAction(newQrItemName, verticalOffset),
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

		private static ModifierAction MoveQrCodeDownAction(string qrItemName, double offsetInPt)
		{
			return new MoveElementDown(qrItemName, ElementType.CustomReportItem, offsetInPt);
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
