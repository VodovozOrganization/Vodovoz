using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using QS.Dialog.GtkUI;
using NHibernate.Util;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ReportsParameters;

namespace Vodovoz.Reports
{
	public partial class SalesReport : SingleUoWWidgetBase, IParametersWidget
	{
		SelectableParametersReportFilter filter;

		public SalesReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			filter = new SelectableParametersReportFilter(UoW);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;

			var nomenclatureTypeParam = filter.CreateEnumParameterSet<NomenclatureCategory>(
				"Типы номенклатур", 
				new ParametersEnumFactory<NomenclatureCategory>(),
				"nomenclature_type"
			);

			var nomenclatureParam = filter.CreateEntityParameterSet<Nomenclature>(
				"Номенклатуры",
				new ParametersFactory<Nomenclature>(UoW, x => x.OfficialName, x => !x.IsArchive), 
				"nomenclature"
			);
			nomenclatureParam.FilterOnSourceSelectionChanged(x => x.Category, nomenclatureTypeParam);

			filter.CreateEntityParameterSet<ProductGroup>(
				"Группы товаров", 
				new RecursiveParametersFactory<ProductGroup>(UoW, x => x.Name, x => x.Childs), 
				"product_group"
			);

			filter.CreateEntityParameterSet<Counterparty>(
				"Контрагенты",
				new ParametersFactory<Counterparty>(UoW, x => x.FullName, x => !x.IsArchive),
				"counterparty"
			);

			filter.CreateEntityParameterSet<Organization>(
				"Организации",
				new ParametersFactory<Organization>(UoW, x => x.FullName),
				"organization"
			);

			filter.CreateEntityParameterSet<DiscountReason>(
				"Основания скидок",
				new ParametersFactory<DiscountReason>(UoW, x => x.Name),
				"discount_reason"
			);

			filter.CreateEntityParameterSet<Subdivision>(
				"Подразделения",
				new ParametersFactory<Subdivision>(UoW, x => x.Name),
				"subdivision"
			);

			filter.CreateEntityParameterSet<Employee>(
				"Авторы заказов",
				new ParametersFactory<Employee>(UoW, x => x.LastName, x => !x.IsFired),
				"order_author"
			);

			filter.CreateEntityParameterSet<GeographicGroup>(
				"Части города",
				new ParametersFactory<GeographicGroup>(UoW, x => x.Name),
				"geographic_group"
			);

			filter.CreateEnumParameterSet<PaymentType>(
				"Тип оплаты",
				new ParametersEnumFactory<PaymentType>(),
				"payment_type"
			);

			filter.CreateEntityParameterSet<PromotionalSet>(
				"Промонаборы",
				new ParametersFactory<PromotionalSet>(UoW, x => x.Name, x => !x.IsArchive),
				"promotional_set"
			);

			var viewModel = new SelectableParameterReportFilterViewModel(filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по продажам";

		#endregion


		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{"creation_date", DateTime.Now}
			};
			foreach(var item in filter.GetParameters()) {
				parameters.Add(item.Key, item.Value);

			}

			return new ReportInfo {
				Identifier = ycheckbuttonDetail.Active ? "Sales.SalesReportDetail" : "Sales.SalesReport",
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}
	}
}