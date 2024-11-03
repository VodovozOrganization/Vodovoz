using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Project.DB;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnedTareReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly SelectableParametersReportFilter _filter;

		#region IParametersWidget implementation

		public string Title => "Отчет по забору тары";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		public ReturnedTareReport(IReportInfoFactory reportInfoFactory, IInteractiveService interactiveService)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			_filter = new SelectableParametersReportFilter(UoW);
			Build();
			
			btnCreateReport.Clicked += (sender, e) => OnUpdate(true);
			btnCreateReport.Sensitive = false;
			daterangepicker.PeriodChangedByUser += Daterangepicker_PeriodChangedByUser;
			yenumcomboboxDateType.ItemsEnum = typeof(OrderDateType);
			yenumcomboboxDateType.SelectedItem = OrderDateType.CreationDate;
			buttonHelp.Clicked += OnButtonHelpClicked;
			
			ConfigureFilter();
		}

		private void ConfigureFilter()
		{
			var subdivisionsFilter = _filter.CreateParameterSet(
				"Подразделения",
				"subdivision",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Subdivision> resultAlias = null;
					var query = UoW.Session.QueryOver<Subdivision>();
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
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Subdivision>>());
					return query.List<SelectableParameter>();
				})
			);
			
			var orderAuthorsFilter = _filter.CreateParameterSet(
				"Авторы заказов",
				"order_author",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Employee> resultAlias = null;
					var query = UoW.Session.QueryOver<Employee>();

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							var criterion = f();

							if(criterion != null)
							{
								query.Where(criterion);
							}
						}
					}

					var authorProjection = CustomProjections.Concat_WS(
						" ",
						Projections.Property<Employee>(x => x.LastName),
						Projections.Property<Employee>(x => x.Name),
						Projections.Property<Employee>(x => x.Patronymic)
					);

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(authorProjection).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
					var paremetersSet = query.List<SelectableParameter>();

					return paremetersSet;
				})
			);
			
			orderAuthorsFilter.AddFilterOnSourceSelectionChanged(subdivisionsFilter,
				() =>
				{
					var selectedValues = subdivisionsFilter.GetSelectedValues().ToArray();

					return !selectedValues.Any()
						? null
						: subdivisionsFilter.FilterType == SelectableFilterType.Include
							? Restrictions.On<Employee>(x => x.Subdivision).IsIn(selectedValues)
							: Restrictions.On<Employee>(x => x.Subdivision).Not.IsIn(selectedValues);
				}
			);

			var viewModel = new SelectableParameterReportFilterViewModel(_filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxMultiParameters.Add(filterWidget);
			filterWidget.Show();
		}


		private void OnButtonHelpClicked(object sender, EventArgs e)
		{
			var info =
				"В отчёт попадают заказы с учётом выбранных фильтров, а также следующих условий:\n" +
				"- есть возвращённые бутыли\n" +
				"- отстуствуют тмц категории \"Вода\" с объёмом тары 19л.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		private void Daterangepicker_PeriodChangedByUser(object sender, EventArgs e) =>
			btnCreateReport.Sensitive = daterangepicker.EndDateOrNull.HasValue && daterangepicker.StartDateOrNull.HasValue;
		
		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", daterangepicker.StartDate },
				{ "end_date", daterangepicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
				{ "date", DateTime.Now },
				{ "date_type", ((OrderDateType)yenumcomboboxDateType.SelectedItem) == OrderDateType.CreationDate },
				{ "is_closed_order_only", chkClosedOrdersOnly.Active }
			};
			
			foreach(var item in _filter.GetParameters())
			{
				parameters.Add(item.Key, item.Value);
			}

			var identifier = "Bottles.ReturnedTareReport";
			var reportInfo = _reportInfoFactory.Create(identifier, Title, parameters);

			return reportInfo;
		}

		private void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
	}
}
