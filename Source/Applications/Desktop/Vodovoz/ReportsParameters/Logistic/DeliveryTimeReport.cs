using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IInteractiveService interactiveService;

		public DeliveryTimeReport(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService)
		{
			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			Build();
			ConfigureDlg();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет 'Время доставки'";

		#endregion

		private void ConfigureDlg()
		{
			ytreeGeoGroups.HeadersVisible = false;
			ytreeGeoGroups.ColumnsConfig = FluentColumnsConfig<SelectableParameter>.Create()
				.AddColumn("").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("").AddTextRenderer(x => x.GeographicGroup.Name)
				.Finish();
			ytreeGeoGroups.ItemsDataSource =
				UoW.GetAll<GeoGroup>().Select(x => new SelectableParameter { GeographicGroup = x, IsSelected = true }).ToList();

			ytreeRouteListTypeOfUse.HeadersVisible = false;
			ytreeRouteListTypeOfUse.ColumnsConfig = FluentColumnsConfig<SelectableParameter>.Create()
				.AddColumn("").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("").AddEnumRenderer(x => x.RouteListTypeOfUse)
				.Finish();
			ytreeRouteListTypeOfUse.ItemsDataSource = Enum.GetValues(typeof(RouteListTypeOfUse)).Cast<RouteListTypeOfUse>()
				.Select(x => new SelectableParameter { RouteListTypeOfUse = x, IsSelected = x == RouteListTypeOfUse.Delivery}).ToList();

			new List<string>() {
				"Группировка по водителям, без нумерации",
				"Без группировки, с нумерацией"
			}.ForEach(comboboxReportType.AppendText);

			comboboxReportType.Active = 0;
		}

		private void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = comboboxReportType.Active == 0 ? "Logistic.DeliveryTimeGrouped" : "Logistic.DeliveryTime",
				Parameters = new Dictionary<string, object>
				{
					{ "beforeTime", ytimeDelivery.Text },
					{ "geographic_groups", GetSelectedGeoGroupIds() },
					{ "rl_type_of_use", GetSelectedRouteListTypeOfUses() },
					{ "filters_text", GetSelectedFilters() },
					{ "creation_date", DateTime.Now }
				}
			};
		}

		private string GetSelectedFilters()
		{
			var selectedGeoGroups = String.Join(", ", GetSelectedGeoGroups().Select(x => x.Name));
			var selectedRouteListTypeOfUses =  String.Join(", ", GetSelectedRouteListTypeOfUses().Select(x => x.GetEnumTitle()));

			return "Выбранные фильтры:\n" +
				$"Время доставки до: {ytimeDelivery.Text}\n" +
				$"Часть города: {selectedGeoGroups}\n" +
				$"Принадлежность МЛ: {selectedRouteListTypeOfUses}\n";
		}

		private void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(!GetSelectedGeoGroupIds().Any())
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана ни одна часть города");
				return;
			}
			if(!GetSelectedRouteListTypeOfUses().Any())
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана ни одна принадлежность МЛ");
				return;
			}
			OnUpdate(true);
		}

		private IEnumerable<RouteListTypeOfUse> GetSelectedRouteListTypeOfUses()
		{
			return (ytreeRouteListTypeOfUse.ItemsDataSource as IEnumerable<SelectableParameter>)
				?.Where(x => x.IsSelected)
				.Select(x => x.RouteListTypeOfUse)
				?? new List<RouteListTypeOfUse>();
		}

		private IEnumerable<int> GetSelectedGeoGroupIds()
		{
			return GetSelectedGeoGroups().Select(x => x.Id);
		}

		private IEnumerable<GeoGroup> GetSelectedGeoGroups()
		{
			return (ytreeGeoGroups.ItemsDataSource as IEnumerable<SelectableParameter>)
				?.Where(x => x.IsSelected)
				.Select(x => x.GeographicGroup)
				?? new List<GeoGroup>();
		}

		private void OnYtimeDeliveryChanged(object sender, EventArgs e)
		{
			buttonCreateReport.Sensitive = ytimeDelivery.Time != default(TimeSpan);
		}

		private class SelectableParameter
		{
			public bool IsSelected { get; set; }
			public GeoGroup GeographicGroup { get; set; }
			public RouteListTypeOfUse RouteListTypeOfUse { get; set; }
		}

		private enum RouteListTypeOfUse
		{
			[Display(Name = "Доставка")]
			Delivery,
			[Display(Name = "СЦ")]
			ServiceCenter,
			[Display(Name = "Фуры")]
			CompanyTrucks,
			[Display(Name = "Складская логистика")]
			StorageLogistics
		}
	}
}
