using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Logistic;
using NHibernate.Transform;
using System.Linq;
using NHibernate.Criterion;
using Vodovoz.ViewModels.Reports;
using QS.Project.Services;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OnecCommentsReport : SingleUoWWidgetBase, IParametersWidget
	{
		SelectableParametersReportFilter _filter;
		private readonly IReportInfoFactory _reportInfoFactory;

		public OnecCommentsReport(IReportInfoFactory reportInfoFactory)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			this.Build ();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot ();
			_filter = new SelectableParametersReportFilter(UoW);
			ConfigureReport();
		}

		private void ConfigureReport()
		{
			var geoGroups = _filter.CreateParameterSet(
				"Части города",
				"geographic_groups",
				new ParametersFactory(UoW, (filters) => {
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

			District districtAlias = null;
			DistrictsSet districtsSetAlias = null;
			GeoGroup geoGroupAlias = null;
			var districtParameter = _filter.CreateParameterSet(
				"Районы",
				"districts",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<District> resultAlias = null;

					var query = UoW.Session.QueryOver(() => districtAlias)
						.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
						.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geoGroupAlias)
						.Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active);

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							var filterCriterion = f();
							if(filterCriterion != null)
							{
								query.Where(filterCriterion);
							}
						}
					}

					query.SelectList(list => list
							.Select(() => districtAlias.Id).WithAlias(() => resultAlias.EntityId)
							.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.EntityTitle)
						);
					var result = query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<District>>())
						.List<SelectableEntityParameter<District>>();
					foreach(var parameter in result)
					{
						parameter.EntityTitle = $"{parameter.EntityId} {parameter.EntityTitle}";
					}
					return result.Cast<SelectableParameter>().ToList();
				})
			);
			districtParameter.AddFilterOnSourceSelectionChanged(geoGroups, () => {
				var selectedValues = geoGroups.GetSelectedValues();
				if(!selectedValues.Any())
				{
					return null;
				}
				return Restrictions.On(() => geoGroupAlias.Id).IsIn(selectedValues.ToArray());
			});

			var viewModel = new SelectableParameterReportFilterViewModel(_filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxSelectableFilter.Add(filterWidget);
			filterWidget.Show();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по комментариям для логистов";
			}
		}

		#endregion

		private ReportInfo GetReportInfo ()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDate.AddDays(1).AddTicks(-1) }
			};

			foreach(var item in _filter.GetParameters())
			{
				parameters.Add(item.Key, item.Value);
			}

			var reportInfo = _reportInfoFactory.Create("Orders.OnecComments", Title, parameters);
			reportInfo.UseUserVariables = true;

			return reportInfo;
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			OnUpdate (true);
		}

		void OnUpdate (bool hide = false)
		{
			if (LoadReport != null) {
				LoadReport (this, new LoadReportEventArgs (GetReportInfo (), hide));
			}
		}

		void CanRun ()
		{
			buttonCreateReport.Sensitive =
				(dateperiodpicker.EndDateOrNull != null && dateperiodpicker.StartDateOrNull != null);
		}

		protected void OnDateperiodpickerPeriodChanged (object sender, EventArgs e)
		{
			CanRun ();
		}

	}
}
