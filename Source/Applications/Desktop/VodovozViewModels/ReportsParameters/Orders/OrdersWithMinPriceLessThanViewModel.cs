using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class OrdersWithMinPriceLessThanViewModel : ReportParametersViewModelBase
	{
		public OrdersWithMinPriceLessThanViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет по заказам с минимальной ценой меньше 100р.";
			Identifier = "Orders.OrdersWithMinPriceLessThan";

			GenerateReportCommand = new DelegateCommand(LoadReport);
		}

		public DelegateCommand GenerateReportCommand;

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>();
				return parameters;
			}
		}
	}
}
