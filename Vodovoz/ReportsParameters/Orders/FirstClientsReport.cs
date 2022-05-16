using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using System.Linq;
using Vodovoz.Domain.Orders;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.Project.Journal.EntitySelector;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Domain.Client;

namespace Vodovoz.ReportsParameters.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FirstClientsReport : SingleUoWWidgetBase, IParametersWidget
	{
		public FirstClientsReport(
			IEntityAutocompleteSelectorFactory districtAutocompleteSelectorFactory,
			IDiscountReasonRepository discountReasonRepository)
		{
			var districtSelector = districtAutocompleteSelectorFactory ??
			                       throw new ArgumentNullException(nameof(districtAutocompleteSelectorFactory));
			
			if(discountReasonRepository == null)
			{
				throw new ArgumentNullException(nameof(discountReasonRepository));
			}
			
			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			var reasons = discountReasonRepository.GetDiscountReasons(UoW);
			yCpecCmbDiscountReason.ItemsList = reasons;
			yCpecCmbDiscountReason.SelectedItem = reasons.FirstOrDefault(r => r.Id == 16);

			yChooseOrderStatus.ItemsEnum = typeof(OrderStatus);
			yChooseOrderStatus.ShowSpecialStateAll = true;

			yChooseThePaymentTypeForTheOrder.ItemsEnum = typeof(PaymentType);
			yChooseThePaymentTypeForTheOrder.ShowSpecialStateAll = true;

			datePeriodPicker.StartDate = datePeriodPicker.EndDate = DateTime.Today;
			entryDistrict.SetEntityAutocompleteSelectorFactory(districtSelector);
			entryDistrict.CanEditReference = false;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по первичным клиентам";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var reportInfo = new ReportInfo
			{
				Identifier = "Orders.FirstClients",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", datePeriodPicker.StartDateOrNull.Value },
					{ "end_date", datePeriodPicker.EndDateOrNull.Value },
					{ "discount_id", (yCpecCmbDiscountReason.SelectedItem as DiscountReason)?.Id ?? 0 },
					{ "order_status", yChooseOrderStatus.SelectedItem.ToString() },
					{ "payment_type", yChooseThePaymentTypeForTheOrder.SelectedItem.ToString() },
					{ "district_id", entryDistrict.Subject?.GetIdOrNull() },
					{ "has_promotional_sets", chkBtnWithPromotionalSets.Active }
				}
			};
			return reportInfo;
		}

		protected void OnDatePeriodPickerPeriodChanged(object sender, EventArgs e)
		{
			SetSensitivity();
		}

		private void SetSensitivity()
		{
			var datePeriodSelected = datePeriodPicker.EndDateOrNull.HasValue && datePeriodPicker.StartDateOrNull.HasValue;
			buttonRun.Sensitive = datePeriodSelected;
		}
	}
}
