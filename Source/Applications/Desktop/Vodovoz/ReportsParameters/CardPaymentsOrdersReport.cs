using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using System.ComponentModel;

namespace Vodovoz.ReportsParameters
{
	[ToolboxItem(true)]
	public partial class CardPaymentsOrdersReport : SingleUoWWidgetBase, IParametersWidget
	{
		public CardPaymentsOrdersReport(IUnitOfWorkFactory unitOfWorkFactory)
		{
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
			return new ReportInfo
			{
				Identifier = "Orders.CardPaymentsOrdersReport",
				Parameters = new Dictionary<string, object>
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
		}


		private void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}
	}
}
