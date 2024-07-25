using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.EntityRepositories.Payments;
using QS.Project.Services;

namespace Vodovoz.ReportsParameters.Payments
{
	public partial class PaymentsFromTinkoffReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IPaymentsRepository _paymentsRepository;
		
		public PaymentsFromTinkoffReport(IPaymentsRepository paymentsRepository)
		{
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));

			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
            dateperiodpicker.StartDate = DateTime.Today.AddDays(-1);
            dateperiodpicker.EndDate = DateTime.Today;
            rbtnYesterday.Active = true;
			SetControlsAccessibility();
			rbtnLast3Days.Clicked += OnRbtnLast3DaysToggled;
			rbtnYesterday.Clicked += OnRbtnYesterdayToggled;
			rbtnCustomPeriod.Clicked += OnCustomPeriodChanged;
            dateperiodpicker.PeriodChangedByUser += OnCustomPeriodChanged;
			ySCmbShop.SetRenderTextFunc<string>(o => string.IsNullOrWhiteSpace(o) ? "{ нет названия }" : o);
			ySCmbShop.ItemsList =_paymentsRepository.GetAllShopsFromTinkoff(UoW);
		}

		void SetControlsAccessibility()
		{
            dateperiodpicker.Sensitive = rbtnCustomPeriod.Active;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по оплатам OnLine заказов";

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
			var rInfo = new ReportInfo {
				Identifier = "Payments.PaymentsFromTinkoffReport",
				Parameters = new Dictionary<string, object> {
					{ "startDate", dateperiodpicker.StartDate },
                    { "endDate", dateperiodpicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
                    { "shop", ySCmbShop.SelectedItem ?? "ALL" }
				}
			};
			return rInfo;
		}

		protected void OnRbtnLast3DaysToggled(object sender, EventArgs e)
		{
            if (rbtnLast3Days.Active)
            {
                dateperiodpicker.StartDate = DateTime.Today.AddDays(-3);
                dateperiodpicker.EndDate = DateTime.Today;
            }

            SetControlsAccessibility();
		}

		protected void OnRbtnYesterdayToggled(object sender, EventArgs e)
		{
			if(rbtnYesterday.Active)
            {
                dateperiodpicker.StartDate = DateTime.Today.AddDays(-1);
                dateperiodpicker.EndDate = DateTime.Today;
            }
            SetControlsAccessibility();
		}

		protected void OnCustomPeriodChanged(object sender, EventArgs e)
		{
			SetControlsAccessibility();
		}
	}
}
