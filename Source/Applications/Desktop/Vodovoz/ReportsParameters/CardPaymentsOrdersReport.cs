using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using System.ComponentModel;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[ToolboxItem(true)]
	public partial class CardPaymentsOrdersReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public CardPaymentsOrdersReport(ReportFactory reportFactory, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			Build();
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			ydateperiodpicker.StartDate = DateTime.Now.Date;
			ydateperiodpicker.EndDate = DateTime.Now.Date;
			comboPaymentFrom.ItemsList = UoW.GetAll<PaymentFrom>();
			comboGeoGroup.ItemsList = UoW.GetAll<GeoGroup>();

			comboPaymentFrom.ItemSelected += (sender, e) =>
			{
				if(comboPaymentFrom.IsSelectedAll)
				{
					ycheckbuttonShowArchive.Sensitive = true;
				}
				else
				{
					ycheckbuttonShowArchive.Sensitive = false;
					ycheckbuttonShowArchive.Active = false;
				}
			};
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по оплатам по картам";

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", ydateperiodpicker.StartDate },
				{ "end_date", ydateperiodpicker.EndDate },
				{
					{ "start_date", ydateperiodpicker.StartDate },
					{ "end_date", ydateperiodpicker.EndDate },
					{
						"payment_from_id",
						comboPaymentFrom.IsSelectedAll ? "" : ((PaymentFrom)comboPaymentFrom.SelectedItem).Id.ToString()
					},
					{
						"geo_group_id",
						comboGeoGroup.IsSelectedAll ? "" : ((GeoGroup)comboGeoGroup.SelectedItem).Id.ToString()
					},
					{ "ShowArchived", !ycheckbuttonShowArchive.Sensitive || ycheckbuttonShowArchive.Active }
				}
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Orders.CardPaymentsOrdersReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}


		private void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}
	}
}
