using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QS.Services;
using QSReport;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Reports
{
	public partial class EquipmentReport : SingleUoWWidgetBase, IParametersWidget
	{
		private SelectableParametersReportFilter filter;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IInteractiveService _interactiveService;

		public EquipmentReport(IReportInfoFactory reportInfoFactory, IInteractiveService interactiveService)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			filter = new SelectableParametersReportFilter(UoW);
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			buttonHelp.Clicked += ShowInfoWindow;

			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;

			filter.CreateParameterSet(
				"Части города",
				"geographic_group",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<GeoGroup> resultAlias = null;
					var query = UoW.Session.QueryOver<GeoGroup>();

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

					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<GeoGroup>>());

					return query.List<SelectableParameter>();
				})
			);

			filter.CreateParameterSet(
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

			filter.CreateParameterSet(
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
							query.Where(f());
						}
					}

					IProjection authorProjection = Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?2, ?1, ?3)"),
						NHibernateUtil.String,
						Projections.Property<Employee>(x => x.Name),
						Projections.Property<Employee>(x => x.LastName),
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

			filter.CreateParameterSet(
				"Контрагенты",
				"counterparty",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Counterparty> resultAlias = null;
					var query = UoW.Session.QueryOver<Counterparty>()
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
						.Select(x => x.FullName).WithAlias(() => resultAlias.EntityTitle)
					);

					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Counterparty>>());

					return query.List<SelectableParameter>();
				})
			);

			var viewModel = new SelectableParameterReportFilterViewModel(filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по оборудованию";

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
				{ "end_date", dateperiodpicker.EndDateOrNull }
			};

			foreach(var item in filter.GetParameters())
			{
				parameters.Add(item.Key, item.Value);
			}

			var reportInfo = _reportInfoFactory.Create("ServiceCenter.EquipmentReport", Title, parameters);

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDate != default(DateTime))
			{
				OnUpdate(true);
			}
			else
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату.");
			}
		}

		private void ShowInfoWindow(object sender, EventArgs e)
		{
			var info = "В отчёт выводятся заказы, в которых присутствует ТМЦ типа \"Оборудование\" в таблице \"Оборудование\".\n" +
					   "Фильтры:\n" +
					   "Дата доставки - дата доставки заказа;\n" +
					   "Части города - часть города точки доставки;\n" +
					   "Подразделения - подразделение автора заказа;\n" +
					   "Авторы заказов - ФИО автора заказа;\n" +
			           "Контрагенты - клиент.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}
	}
}

