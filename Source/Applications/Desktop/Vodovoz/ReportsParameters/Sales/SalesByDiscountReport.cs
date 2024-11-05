using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ReportsParameters.Sales
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SalesByDiscountReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly SelectableParametersReportFilter _filter;
		private readonly IReportInfoFactory _reportInfoFactory;

		public SalesByDiscountReport(IReportInfoFactory reportInfoFactory)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			_filter = new SelectableParametersReportFilter(UoW);
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			ConfigureMultipleFilter();
			yenumcomboboxDateType.ItemsEnum = typeof(OrderDateType);
			yenumcomboboxDateType.SelectedItem = OrderDateType.DeliveryDate;
		}

		private void ConfigureMultipleFilter()
		{
			_filter.CreateParameterSet(
				"Основания скидок",
				"discount_reason",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<DiscountReason> resultAlias = null;
					var query = UoW.Session.QueryOver<DiscountReason>();
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<DiscountReason>>());
					return query.List<SelectableParameter>();
				})
			);

			_filter.CreateParameterSet(
				"Промонаборы",
				"promotional_set",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<PromotionalSet> resultAlias = null;
					var query = UoW.Session.QueryOver<PromotionalSet>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<PromotionalSet>>());
					return query.List<SelectableParameter>();
				})
			);

			var viewModel = new SelectableParameterReportFilterViewModel(_filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxMultipleFilter.Add(filterWidget);
			filterWidget.Show();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по скидкам";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
				{
					{ "StartDate", dateperiodpicker.StartDateOrNull },
					{ "EndDate", dateperiodpicker.EndDateOrNull },
					{ "DateType", yenumcomboboxDateType.SelectedItem.ToString()}
				};

			foreach(var item in _filter.GetParameters())
			{
				parameters.Add(item.Key, item.Value);
			}

			var reportInfo = _reportInfoFactory.Create("Sales.SalesByDiscountReport", Title, parameters);
			return reportInfo;
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

	public enum OrderDateType
	{
		[Display(Name = "По дате доставки")]
		DeliveryDate,
		[Display(Name = "По дате создания")]
		CreationDate
	}
}
