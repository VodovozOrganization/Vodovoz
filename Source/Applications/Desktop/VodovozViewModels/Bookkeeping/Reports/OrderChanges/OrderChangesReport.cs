using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Reports;
using Vodovoz.Settings.Orders;
using static Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges.OrderChangesReportViewModel;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	[Appellative(Nominative = "Отчет по изменениям заказа при доставке")]
	public partial class OrderChangesReport : IClosedXmlReport
	{
		private const string _dateFormatString = "yyyy-MM-dd";
		private const string _dateAndTimeFormatString = "yyyy-MM-dd hh:mm:ss";

		private DateTime _startDate;
		private DateTime _endDate;
		private bool _isOldMonitoring;
		private Organization _selectedOrganization;
		private IEnumerable<SelectableKeyValueNode> _selectedChangeTypes = new List<SelectableKeyValueNode>();
		private IEnumerable<SelectableKeyValueNode> _selectedIssueTypes = new List<SelectableKeyValueNode>();

		private OrderChangesReport(
			IOrderSettings orderSettings,
			DateTime startDate,
			DateTime endDate,
			bool isOldMonitoring,
			Organization selectedOrganization,
			IEnumerable<SelectableKeyValueNode> selectedChangeTypes,
			IEnumerable<SelectableKeyValueNode> selectedIssueTypes)
		{
			_startDate = startDate;
			_endDate = endDate;
			_isOldMonitoring = isOldMonitoring;
			_selectedOrganization = selectedOrganization;
			_selectedChangeTypes = selectedChangeTypes;
			_selectedIssueTypes = selectedIssueTypes;

			_isPaymentTypeChangeTypeSelected =
				_selectedChangeTypes.Select(x => x.Value).Contains("PaymentType");
			_isPriceChangeTypeSelected =
				_selectedChangeTypes.Select(x => x.Value).Contains("Price");
			_isOrderItemsCountChangeSelected =
				_selectedChangeTypes.Select(x => x.Value).Contains("OrderItemsCount");
			_isActualCountChangeSelected =
				_selectedChangeTypes.Select(x => x.Value).Contains("ActualCount");

			_isSmsIssuesTypeSelected =
				_selectedIssueTypes.Select(x => x.Value).Contains("SmsIssues");
			_isQrIssuesTypeSelected =
				_selectedIssueTypes.Select(x => x.Value).Contains("QrIssues");
			_isTerminalIssuesTypeSelected =
				_selectedIssueTypes.Select(x => x.Value).Contains("TerminalIssues");
			_isManagersIssuesTypeSelected =
				_selectedIssueTypes.Select(x => x.Value).Contains("ManagersIssues");

			_paymentByCardFromSmsId = orderSettings.PaymentByCardFromSmsId;
			_paymentByCardFromFastPaymentServiceId = orderSettings.GetPaymentByCardFromFastPaymentServiceId;
		}

		public DateTime StartDate => _startDate;
		public DateTime EndDate => _endDate;
		public bool IsOldMonitoring => _isOldMonitoring;
		public Organization SelectedOrganization => _selectedOrganization;

		public string Title =>
			$"Отчет по изменениям заказа при доставке " +
			$"с {StartDate.ToString(_dateFormatString)} по {EndDate.ToString(_dateFormatString)}";

		public string SelectedOrganizationInfo =>
			$"Организация: {SelectedOrganization.Name}";

		public string SelectedChangeTypesInfo =>
			$"Типы изменений: {string.Join(", ", _selectedChangeTypes.Select(x => x.Key))}";

		public string SelectedIssueTypesInfo =>
			$"Типы проблем: {string.Join(", ", _selectedIssueTypes.Select(x => x.Key))}";

		public string ReportCreationTimeInfo =>
			$"Сформировано {DateTime.Now.ToString(_dateAndTimeFormatString)}";

		public IEnumerable<OrderChangesReportRow> Rows { get; set; } = new List<OrderChangesReportRow>();

		public string TemplatePath => @".\Reports\Orders\OrderChangesReport.xlsx";

		public static async Task<OrderChangesReport> Create(
			IUnitOfWork unitOfWork,
			IOrderSettings orderSettings,
			DateTime startDate,
			DateTime endDate,
			bool isOldMonitoring,
			Organization selectedOrganization,
			IEnumerable<SelectableKeyValueNode> selectedChangeTypes,
			IEnumerable<SelectableKeyValueNode> selectedIssueTypes,
			CancellationToken cancellationToken)
		{
			var report = new OrderChangesReport(
				orderSettings,
				startDate,
				endDate,
				isOldMonitoring,
				selectedOrganization,
				selectedChangeTypes,
				selectedIssueTypes);

			await report.SetReportRows(unitOfWork, cancellationToken);

			return report;
		}
	}
}
