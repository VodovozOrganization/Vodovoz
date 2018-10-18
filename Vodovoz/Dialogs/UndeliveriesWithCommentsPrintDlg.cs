using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using QSTDI;
using Vodovoz.JournalFilters;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveriesWithCommentsPrintDlg : TdiTabBase
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
		string guiltySide = String.Empty;
		int guiltyDepartmentId = 0;
		string newInvoiceCreated = String.Empty;
		string undeliveryStatus = String.Empty;
		int undeliveryAuthorId = 0;

		public UndeliveriesWithCommentsPrintDlg(UndeliveredOrdersFilter filter)
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
				guiltySide = filter.RestrictGuiltySide.Value.ToString();
			if(filter.RestrictGuiltyDepartment != null)
				guiltyDepartmentId = filter.RestrictGuiltyDepartment.Id;
			if(filter.NewInvoiceCreated.HasValue)
				newInvoiceCreated = filter.NewInvoiceCreated.Value ? "true" : "false";
			if(filter.RestrictUndeliveryStatus.HasValue)
				undeliveryStatus = filter.RestrictUndeliveryStatus.ToString();
			if(filter.RestrictUndeliveryAuthor != null)
				undeliveryAuthorId = filter.RestrictUndeliveryAuthor.Id;
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
			var parameters = new Dictionary<string, object>();

			parameters.Add("old_order_id", oldOrderId);
			parameters.Add("driver_id", driverId);
			parameters.Add("client_id", clientId);
			parameters.Add("address_id", addressId);
			parameters.Add("old_order_author_id", oldOrderAuthorId);
			parameters.Add("start_date", oldOrderStartDate);
			parameters.Add("end_date", oldOrderEndDate);
			parameters.Add("new_order_start_date", newOrderStartDate);
			parameters.Add("new_order_end_date", newOrderEndDate);
			parameters.Add("guilty_side", guiltySide);
			parameters.Add("guilty_department_id", guiltyDepartmentId);
			parameters.Add("new_invoice_created", newInvoiceCreated);
			parameters.Add("undelivery_status", undeliveryStatus);
			parameters.Add("undelivery_author_id", undeliveryAuthorId);

			return new ReportInfo {
				Identifier = "Orders.UndeliveriesWithComments",
				UseUserVariables = true,
				Parameters = parameters
			};
		}
	}
}
