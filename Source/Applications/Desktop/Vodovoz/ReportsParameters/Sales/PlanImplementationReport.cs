using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.Project.DB;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ReportsParameters
{
	public partial class PlanImplementationReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly SelectableParametersReportFilter _filter;
		private readonly IReportInfoFactory _reportInfoFactory;
		private const string _orderAuthorIncludeParameter = "order_author_include";
		
		public PlanImplementationReport(IReportInfoFactory reportInfoFactory, bool orderById = false)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			_filter = new SelectableParametersReportFilter(UoW);
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			dateperiodpicker.StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			dateperiodpicker.EndDate = dateperiodpicker.StartDate.AddMonths(1).AddDays(-1);

			var availablePlansToUse = new[] { WageParameterItemTypes.SalesPlan };
			lstCmbPlanType.SetRenderTextFunc<WageParameterItemTypes>(t => t.GetEnumTitle());
			lstCmbPlanType.ItemsList = availablePlansToUse;
			lstCmbPlanType.SelectedItem = availablePlansToUse.FirstOrDefault();
			comboTypeOfDate.ItemsEnum = typeof(OrderDateType);
			comboTypeOfDate.SelectedItem = OrderDateType.CreationDate;
			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
			
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
					EmployeeWageParameter wageParameterAlias = null;
					WageParameterItem wageParameterItemAlias = null;
					
					var query = UoW.Session.QueryOver<Employee>()
						.JoinAlias(e => e.WageParameters, () => wageParameterAlias)
						.JoinAlias(() => wageParameterAlias.WageParameterItem, () => wageParameterItemAlias);

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

					query.Where(e => e.Status == EmployeeStatus.IsWorking)
						.And(e => e.Category == EmployeeCategory.office)
						.And(() => wageParameterAlias.EndDate == null || wageParameterAlias.EndDate >= DateTime.Now)
						.And(() => wageParameterItemAlias.WageParameterItemType == WageParameterItemTypes.SalesPlan);

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

					return query.List<SelectableParameter>();
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
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчёт о выполнении плана";

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{"start_date", dateperiodpicker.StartDateOrNull},
				{"end_date", dateperiodpicker.EndDateOrNull.Value.AddDays(1).AddTicks(-1)},
				{"is_creation_date", (OrderDateType)comboTypeOfDate.SelectedItem == OrderDateType.CreationDate}
			};
			
			foreach(var item in _filter.GetParameters())
			{
				parameters.Add(item.Key, item.Value);
			}

			string identifier;
			//Если не выбран ни один сотрудник, открываем общий отчет, иначе подробный по сотрудникам
			if(parameters.ContainsKey(_orderAuthorIncludeParameter)
				&& parameters[_orderAuthorIncludeParameter] is object[] values
				&& values.Length == 1
				&& values[0] == "0")
			{
				identifier = "Sales.PlanImplementationFullReport";
			}
			else
			{
				identifier = "Sales.PlanImplementationByEmployeeReport";
			}

			var reportInfo = _reportInfoFactory.Create(identifier, Title, parameters);
			return reportInfo;
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDateOrNull == null) {
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать дату");
				return;
			}
			OnUpdate(true);
		}
	}
}
