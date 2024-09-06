using Gamma.ColumnConfig;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersCreationTimeReport : SingleUoWWidgetBase, IParametersWidget
	{
		SelectableParametersReportFilter filter;

		public string Title => "Отчет по времени приема заказов";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public OrdersCreationTimeReport(IReportInfoFactory reportInfoFactory)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			filter = new SelectableParametersReportFilter(UoW);
			buttonCreateReport.Clicked += (sender, e) => OnUpdate(true);

			datepicker.IsEditable = true;

			ytimeentryCreate1.FocusOutEvent += YtimeentryCreate1_FocusOutEvent;
			ytimeentryCreate2.FocusOutEvent += YtimeentryCreate2_FocusOutEvent;
			ytimeentryCreate3.FocusOutEvent += YtimeentryCreate3_FocusOutEvent;

			ytimeentryDeliveryInterval.Changed += (sender, e) => UpdateButtonAddIntervalSensitive();
			ybuttonAddDeliveryInterval.Clicked += (sender, e) => AddTime(ytimeentryDeliveryInterval.Time);
			ybuttonDeleteDeliveryInterval.Clicked += (sender, e) => DeleteTime();
			treeviewDeliveryIntervals.KeyPressEvent += YtreeviewDeliveryIntervals_KeyPressEvent;;

			ConfigureReport();
			SetDefaultValues();
		}

		private void SetDefaultValues()
		{
			ytimeentryCreate1.Time = TimeSpan.Parse("04:00");
			ytimeentryCreate2.Time = TimeSpan.Parse("09:00");
			ytimeentryCreate3.Time = TimeSpan.Parse("12:00");

			Times.Add(new Time(new TimeSpan(10, 00, 00)));
			Times.Add(new Time(new TimeSpan(13, 00, 00)));
			Times.Add(new Time(new TimeSpan(15, 00, 00)));
			Times.Add(new Time(new TimeSpan(18, 00, 00)));
			Times.Add(new Time(new TimeSpan(21, 00, 00)));
			Times.Add(new Time(new TimeSpan(23, 00, 00)));
		}

		private void ConfigureReport()
		{
			treeviewDeliveryIntervals.ColumnsConfig = FluentColumnsConfig<Time>.Create()
				.AddColumn("").AddTextRenderer(x => $"{x.Value:hh\\:mm}")
				.Finish();
			treeviewDeliveryIntervals.ItemsDataSource = Times;
			treeviewDeliveryIntervals.HeadersVisible = false;

			var geoGroups = filter.CreateParameterSet(
				"Части города",
				"geographic_groups",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<GeoGroup> resultAlias = null;
					var query = UoW.Session.QueryOver<GeoGroup>();

					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
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
			var districtParameter = filter.CreateParameterSet(
				"Районы",
				"districts",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<District> resultAlias = null;

					var query = UoW.Session.QueryOver(() => districtAlias)
						.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
						.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geoGroupAlias)
						.Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active);

					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							var filterCriterion = f();
							if(filterCriterion != null) {
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
					foreach (var parameter in result) {
						parameter.EntityTitle = $"{parameter.EntityId} {parameter.EntityTitle}";
					}
					return result.Cast<SelectableParameter>().ToList();
				})
			);
			districtParameter.AddFilterOnSourceSelectionChanged(geoGroups, () => {
				var selectedValues = geoGroups.GetSelectedValues();
				if(!selectedValues.Any()) {
					return null;
				}
				return Restrictions.On(() => geoGroupAlias.Id).IsIn(selectedValues.ToArray());
			});

			var viewModel = new SelectableParameterReportFilterViewModel(filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}

		void YtimeentryCreate1_FocusOutEvent(object sender, EventArgs e)
		{
			if(ytimeentryCreate1.Time > TimeSpan.Parse("23:54")) {
				ytimeentryCreate1.Time = TimeSpan.Parse("23:54");
				ytimeentryCreate2.Time = TimeSpan.Parse("23:56");
				ytimeentryCreate3.Time = TimeSpan.Parse("23:58");
				return;
			}
			if(ytimeentryCreate1.Time >= ytimeentryCreate2.Time) {
				ytimeentryCreate2.Time = ytimeentryCreate1.Time + TimeSpan.FromMinutes(2);
			}
			if(ytimeentryCreate2.Time >= ytimeentryCreate3.Time) {
				ytimeentryCreate3.Time = ytimeentryCreate2.Time + TimeSpan.FromMinutes(2);
			}
		}

		void YtimeentryCreate2_FocusOutEvent(object sender, EventArgs e)
		{
			if(ytimeentryCreate2.Time > TimeSpan.Parse("23:56")) {
				ytimeentryCreate2.Time = TimeSpan.Parse("23:56");
				ytimeentryCreate3.Time = TimeSpan.Parse("23:58");
				return;
			}
			if(ytimeentryCreate2.Time >= ytimeentryCreate3.Time) {
				ytimeentryCreate3.Time = ytimeentryCreate2.Time + TimeSpan.FromMinutes(2);
			}

			if(ytimeentryCreate2.Time < TimeSpan.Parse("00:02")) {
				ytimeentryCreate2.Time = TimeSpan.Parse("00:02");
				return;
			}

			if(ytimeentryCreate2.Time <= ytimeentryCreate1.Time) {
				ytimeentryCreate1.Time = ytimeentryCreate2.Time - TimeSpan.FromMinutes(2);
			}
		}

		void YtimeentryCreate3_FocusOutEvent(object sender, EventArgs e)
		{
			if(ytimeentryCreate3.Time == TimeSpan.Parse("00:00")) {
				ytimeentryCreate3.Time = TimeSpan.Parse("23:59");
			}
			if(ytimeentryCreate3.Time < TimeSpan.Parse("00:04")) {
				ytimeentryCreate3.Time = TimeSpan.Parse("00:04");
				ytimeentryCreate2.Time = TimeSpan.Parse("00:02");
				return;
			}

			if(ytimeentryCreate3.Time <= ytimeentryCreate2.Time) {
				ytimeentryCreate2.Time = ytimeentryCreate3.Time - TimeSpan.FromMinutes(2);
			}
			if(ytimeentryCreate2.Time <= ytimeentryCreate1.Time) {
				ytimeentryCreate1.Time = ytimeentryCreate2.Time - TimeSpan.FromMinutes(2);
			}
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "date", datepicker.Date }
			};
			foreach(var item in filter.GetParameters()) {
				parameters.Add(item.Key, item.Value);

			}

			parameters.Add("creation_time1", $"{ytimeentryCreate1.Time:hh\\:mm}");
			parameters.Add("creation_time2", $"{ytimeentryCreate2.Time:hh\\:mm}");
			parameters.Add("creation_time3", $"{ytimeentryCreate3.Time:hh\\:mm}");

			for(int i = 1; i <= 12; i++) {
				string intervalValue = "";
				if(Times.Count >= i) {
					intervalValue = $"{Times[i - 1].Value:hh\\:mm}";
				}
				parameters.Add($"interval{i}", intervalValue);
			}

			var reportInfo = _reportInfoFactory.Create("Logistic.OrdersCreationTime", Title, parameters);
			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			if(datepicker.DateOrNull == null) {
				MessageDialogHelper.RunWarningDialog("Необходимо выбрать дату");
				return;
			}
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		public GenericObservableList<Time> Times = new GenericObservableList<Time>();
		private readonly IReportInfoFactory _reportInfoFactory;

		private void UpdateButtonAddIntervalSensitive()
		{
			ybuttonAddDeliveryInterval.Sensitive = ytimeentryDeliveryInterval.Time != new TimeSpan() && Times.Count < 12;
		}

		public void AddTime(TimeSpan time)
		{
			if(time == new TimeSpan()) {
				return;
			}
			if(Times.Count >= 12) {
				return;
			}
			if(Times.Any() && Times.Last().Value > time) {
				MessageDialogHelper.RunWarningDialog("Добавляемое время должно быть позже предыдущего");
				return;
			}
			Times.Add(new Time(time));
			UpdateButtonAddIntervalSensitive();
		}

		public void DeleteTime()
		{
			Time selectedTime = treeviewDeliveryIntervals.GetSelectedObject() as Time;
			if(selectedTime == null) {
				return;
			}
			if(Times.Contains(selectedTime)) {
				Times.Remove(selectedTime);
			}
		}

		private void YtreeviewDeliveryIntervals_KeyPressEvent(object o, Gtk.KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Delete) {
				DeleteTime();
			}
		}

		public class Time
		{
			public TimeSpan Value { get; set; }

			public Time(TimeSpan value)
			{
				Value = value;
			}
		}
	}
}
