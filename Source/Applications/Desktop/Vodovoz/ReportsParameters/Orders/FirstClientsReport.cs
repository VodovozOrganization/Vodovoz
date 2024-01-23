using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using System.Linq;
using Vodovoz.Domain.Orders;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.Domain.Logistic;
using Vodovoz.Journals.FilterViewModels;
using QS.Project.Services;

namespace Vodovoz.ReportsParameters.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FirstClientsReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IDistrictJournalFactory _districtJournalFactory;

		public FirstClientsReport(
			IDistrictJournalFactory districtJournalFactory,
			IDiscountReasonRepository discountReasonRepository)
		{
			_districtJournalFactory = districtJournalFactory ?? throw new ArgumentNullException(nameof(districtJournalFactory));
			var districtFilter = new DistrictJournalFilterViewModel { Status = DistrictsSetStatus.Active };
			var districtSelector = _districtJournalFactory.CreateDistrictAutocompleteSelectorFactory(districtFilter);

			if(discountReasonRepository == null)
			{
				throw new ArgumentNullException(nameof(discountReasonRepository));
			}
			
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

			var reasons = discountReasonRepository.GetActiveDiscountReasons(UoW);
			yCpecCmbDiscountReason.ItemsList = reasons;
			yCpecCmbDiscountReason.SelectedItem = reasons?.OrderByDescending(r => r.Id).First() ?? null;

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
