using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Report;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveriesWithCommentsPrintDlg : QS.Dialog.Gtk.TdiTabBase
	{
		int oldOrderId = 0;
		int driverId = 0;
		int clientId = 0;
		int addressId = 0;
		int oldOrderAuthorId = 0;
		string oldOrderStartDate = String.Empty;
		string oldOrderEndDate = String.Empty;
		string newOrderStartDate = String.Empty;
		string newOrderEndDate = String.Empty;
		string[] guiltySides = { "0" };
		int guiltyDepartmentId = 0;
		string newInvoiceCreated = String.Empty;
		string undeliveryStatus = String.Empty;
		int undeliveryAuthorId = 0;
		string oldOrderStatus = String.Empty;

		public UndeliveriesWithCommentsPrintDlg(UndeliveredOrdersFilterViewModel filter)
		{
			this.Build();

			if(filter.RestrictOldOrder != null)
				oldOrderId = filter.RestrictOldOrder.Id;
			if(filter.RestrictDriver != null)
				driverId = filter.RestrictDriver.Id;
			if(filter.RestrictClient != null)
				clientId = filter.RestrictClient.Id;
			if(filter.RestrictAddress != null)
				addressId = filter.RestrictAddress.Id;
			if(filter.RestrictOldOrderAuthor != null)
				oldOrderAuthorId = filter.RestrictOldOrderAuthor.Id;
			if(filter.RestrictOldOrderStartDate.HasValue)
				oldOrderStartDate = filter.RestrictOldOrderStartDate.Value.ToString("s");
			if(filter.RestrictOldOrderEndDate.HasValue)
				oldOrderEndDate = filter.RestrictOldOrderEndDate.Value.ToString("s");
			if(filter.RestrictNewOrderStartDate.HasValue)
				newOrderStartDate = filter.RestrictNewOrderStartDate.Value.ToString("s");
			if(filter.RestrictNewOrderEndDate.HasValue)
				newOrderEndDate = filter.RestrictNewOrderEndDate.Value.ToString("s");
			if(filter.RestrictGuiltySide.HasValue)
				guiltySides = new[] { filter.RestrictGuiltySide.Value.ToString() };
			if(filter.RestrictGuiltyDepartment != null)
				guiltyDepartmentId = filter.RestrictGuiltyDepartment.Id;
			if(filter.NewInvoiceCreated.HasValue)
				newInvoiceCreated = filter.NewInvoiceCreated.Value ? "true" : "false";
			if(filter.RestrictUndeliveryStatus.HasValue)
				undeliveryStatus = filter.RestrictUndeliveryStatus.ToString();
			if(filter.RestrictUndeliveryAuthor != null)
				undeliveryAuthorId = filter.RestrictUndeliveryAuthor.Id;
			if(filter.OldOrderStatus != null)
				oldOrderStatus = filter.OldOrderStatus.Value.ToString();

			if(filter.RestrictIsProblematicCases) {
				guiltySides = Enum.GetValues(typeof(GuiltyTypes))
								  .Cast<GuiltyTypes>()
								  .Where(t => !filter.ExcludingGuiltiesForProblematicCases.Contains(t))
								  .Select(g => g.ToString())
								  .ToArray()
								  ;
			}

			TabName = "Печать недовозов и комментариев";
			Configure();
		}

		void Configure()
		{

			PreviewDocument();
		}

		void PreviewDocument()
		{
			var reportInfo = GetReportInfo();
			reportViewer.LoadReport(reportInfo.GetReportUri(), reportInfo.GetParametersString(), reportInfo.ConnectionString, true);
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object> {
				{ "old_order_id", oldOrderId },
				{ "driver_id", driverId },
				{ "client_id", clientId },
				{ "address_id", addressId },
				{ "old_order_author_id", oldOrderAuthorId },
				{ "start_date", oldOrderStartDate },
				{ "end_date", oldOrderEndDate },
				{ "new_order_start_date", newOrderStartDate },
				{ "new_order_end_date", newOrderEndDate },
				{ "guilty_sides", guiltySides },
				{ "guilty_department_id", guiltyDepartmentId },
				{ "new_invoice_created", newInvoiceCreated },
				{ "undelivery_status", undeliveryStatus },
				{ "undelivery_author_id", undeliveryAuthorId },
				{ "old_order_status", oldOrderStatus },
				{ "are_guilties_filtred", guiltySides.Any(x => x == "0") }
			};

			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Identifier = "Orders.UndeliveriesWithComments";
			reportInfo.UseUserVariables = true;
			reportInfo.Parameters = parameters;
			return reportInfo;
		}
	}
}
